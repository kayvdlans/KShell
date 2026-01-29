using KShell.Interfaces;

namespace KShell.Commands;

public sealed class PwdCommand : ICommand
{
    public void Run(TextReader stdin, TextWriter stdout, TextWriter stderr, params string[] args)
    {
        stdout.WriteLine(Environment.CurrentDirectory);
    }
}