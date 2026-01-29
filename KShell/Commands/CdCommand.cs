using KShell.Extensions;
using KShell.Interfaces;

namespace KShell.Commands;

public sealed class CdCommand : ICommand
{
    public void Run(TextReader stdin, TextWriter stdout, TextWriter stderr, params string[] args)
    {
        if (args.Length == 0) return;

        args[0] = args[0].ToPath();

        if (!Directory.Exists(args[0]))
        {
            stderr.WriteLine($"cd: {args[0]}: No such file or directory");
            return;
        }

        Environment.CurrentDirectory = args[0];
    }
}