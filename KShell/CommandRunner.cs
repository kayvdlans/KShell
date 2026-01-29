using System.Buffers;
using System.IO.Pipelines;
using KShell.Commands;
using KShell.Extensions;
using KShell.Interfaces;
using KShell.Utils;

namespace KShell;

public static class CommandRunner
{
    public static readonly Dictionary<string, ICommand> Commands = new()
    {
        { "cd", new CdCommand() },
        { "echo", new EchoCommand() },
        { "exit", new ExitCommand() },
        { "history", new HistoryCommand() },
        { "type", new TypeCommand() },
        { "pwd", new PwdCommand() }
    };

    public static IEnumerable<string> GetPossibleCommands(string name)
    {
        return Commands.Keys.Where(cmd => cmd.StartsWith(name));
    }

    private static void PreprocessSegment(PipelineSegment segment)
    {
        var tokens = segment.Tokens;
        var skip = new bool[tokens.Length];

        for (var i = 0; i < tokens.Length; i++)
        {
            var token = tokens[i].Trim();
            switch (token)
            {
                case ">":
                case "1>":
                    AddToTargets(tokens[i + 1], segment.RedirectTargets.StdOut, ref i);
                    break;
                case "2>":
                    AddToTargets(tokens[i + 1], segment.RedirectTargets.StdErr, ref i);
                    break;

                case ">>":
                case "1>>":
                    AddToTargets(tokens[i + 1], segment.AppendTargets.StdOut, ref i);
                    break;

                case "2>>":
                    AddToTargets(tokens[i + 1], segment.AppendTargets.StdErr, ref i);
                    break;
                default:
                    continue;
            }
        }

        segment.Tokens = [.. tokens.Where((_, idx) => !skip[idx])];
        return;

        void AddToTargets(string target, List<string> targets, ref int i)
        {
            if (i + 1 >= tokens.Length)
                return;

            targets.Add(target);
            skip[i] = true;
            skip[++i] = true;
        }
    }

    public static async Task RunAsync(string input)
    {
        var segments = new InputReader().Read(input).ToArray();

        var tasks = new List<Task>();

        PipeReader? currentIn = null;

        for (var i = 0; i < segments.Length; i++)
        {
            var segment = segments[i];
            PreprocessSegment(segment);

            if (segment.Tokens.Length == 0)
                return;

            var isLastSegment = i == segments.Length - 1;
            var argv = segment.Tokens;

            var outPipe = isLastSegment ? null : new Pipe();

            var stdin = currentIn != null ? currentIn.AsTextReader() : Console.In;
            TextWriter stdout;
            if (!isLastSegment)
                stdout = outPipe!.Writer.AsTextWriter();
            else
                stdout = segment.RedirectTargets.WriteStdOut || segment.AppendTargets.WriteStdOut
                    ? new StringWriter()
                    : Console.Out;

            var stderr = new StringWriter();

            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    if (Commands.TryGetValue(argv[0], out var command))
                        command.Run(stdin, stdout, stderr, argv[1..]);
                    else if (ExecutableUtils.Exists(argv[0]))
                        await RunExternalCommand(argv, stdin, stdout, stderr);
                    else
                        await stderr.WriteLineAsync($"{argv[0]}: command not found");
                }
                finally
                {
                    await stdout.DisposeAsync();
                    stdin.Dispose();

                    WriteOutput(stderr.ToString(),
                        segment.RedirectTargets.StdErr,
                        segment.AppendTargets.StdErr,
                        Console.Error);
                }

                if (isLastSegment && stdout is StringWriter sw)
                {
                    WriteOutput(sw.ToString(),
                        segment.RedirectTargets.StdOut,
                        segment.AppendTargets.StdOut,
                        Console.Out);
                }
                else
                {
                    var stdoutContent = stdout.ToString() ?? string.Empty;
                    if (segment.RedirectTargets.WriteStdOut)
                        WriteToTargets(stdoutContent, true, segment.RedirectTargets.StdOut);
                    if (segment.AppendTargets.WriteStdOut)
                        WriteToTargets(stdoutContent, false, segment.AppendTargets.StdOut);
                }
            }));

            currentIn = outPipe?.Reader;
        }

        await Task.WhenAll(tasks);
    }


    private static async Task RunExternalCommand(string[] argv, TextReader stdin, TextWriter stdout, TextWriter stderr)
    {
        var hasPipedInput = stdin != Console.In;
        var process = ExecutableUtils.RunExecutable(argv[0], hasPipedInput, argv[1..]);
        if (process is null)
            return;

        var stdinTask = Task.CompletedTask;
        if (hasPipedInput)
            stdinTask = ReadStreamAsync(stdin, process.StandardInput);

        var stdoutTask = ReadStreamAsync(process.StandardOutput, stdout);
        var stderrTask = ReadStreamAsync(process.StandardError, stderr);

        await Task.WhenAll(stdinTask, stdoutTask, stderrTask, process.WaitForExitAsync());
    }

    private static async Task ReadStreamAsync(TextReader reader, TextWriter writer)
    {
        var buffer = ArrayPool<char>.Shared.Rent(8096);

        try
        {
            while (true)
            {
                var n = await reader.ReadAsync(buffer, 0, buffer.Length);
                if (n == 0)
                    break;

                await writer.WriteAsync(buffer, 0, n);
                await writer.FlushAsync();
            }
        }
        catch (IOException)
        {
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
            writer.Close();
        }
    }

    private static void WriteOutput(
        string content,
        List<string> redirectTargets,
        List<string> appendTargets,
        TextWriter defaultOutput)
    {
        var hasRedirect = redirectTargets.Count > 0;
        var hasAppend = appendTargets.Count > 0;

        if (!hasRedirect && !hasAppend)
        {
            if (!string.IsNullOrEmpty(content))
                defaultOutput.Write(content);
        }
        else
        {
            WriteToTargets(content, true, redirectTargets);
            WriteToTargets(content, false, appendTargets);
        }
    }

    private static void WriteToTargets(string content, bool overwrite, IEnumerable<string> targets)
    {
        foreach (var target in targets)
            if (overwrite)
                FileUtils.WriteToFile(target, content);
            else
                FileUtils.AppendToFile(target, content);
    }
}