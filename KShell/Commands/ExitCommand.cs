using KShell.Interfaces;

namespace KShell.Commands;

public sealed class ExitCommand : ICommand
{
    public void Run(TextReader stdin, TextWriter stdout, TextWriter stderr, params string[] args)
    {
        Environment.Exit(0);
    }
}