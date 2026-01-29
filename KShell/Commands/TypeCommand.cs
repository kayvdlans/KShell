using KShell.Interfaces;
using KShell.Utils;

namespace KShell.Commands;

public sealed class TypeCommand : ICommand
{
    public void Run(TextReader stdin, TextWriter stdout, TextWriter stderr, params string[] args)
    {
        foreach (var arg in args)
            if (CommandRunner.Commands.ContainsKey(arg))
                stdout.WriteLine($"{arg} is a shell builtin");
            else if (ExecutableUtils.FindExecutableInPath(arg, out var path))
                stdout.WriteLine($"{arg} is {path}");
            else
                stdout.WriteLine($"{arg}: not found");
    }
}