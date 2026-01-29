using KShell.Interfaces;

namespace KShell.Commands;

public sealed class HistoryCommand : ICommand
{
    public void Run(TextReader stdin, TextWriter stdout, TextWriter stderr, params string[] args)
    {
        var start = 0;
        if (args.Length == 1 && int.TryParse(args[0], out var len))
            start = History.All.Count - len;

        var writeOutput = true;

        for (var i = 0; i < args.Length; i++)
            switch (args[i])
            {
                case "-r" when i + 1 < args.Length:
                    try
                    {
                        writeOutput = false;
                        History.ReadFromFile(args[i + 1]);
                    }
                    catch (Exception e)
                    {
                        stderr.WriteLine(e.Message);
                    }

                    break;
                case "-w" when i + 1 < args.Length:
                    try
                    {
                        writeOutput = false;
                        History.WriteToFile(args[i + 1]);
                    }
                    catch (Exception e)
                    {
                        stderr.WriteLine(e.Message);
                    }

                    break;

                case "-a" when i + 1 < args.Length:
                    try
                    {
                        writeOutput = false;
                        History.AppendToFile(args[i + 1]);
                    }
                    catch (Exception e)
                    {
                        stderr.WriteLine(e);
                    }

                    break;
            }

        if (!writeOutput)
            return;

        for (var i = start; i < History.All.Count; i++)
            Console.WriteLine($"    {i + 1}  {History.All[i]}");
    }
}