using KShell.Interfaces;

namespace KShell.Commands;

public sealed class EchoCommand : ICommand
{
    public void Run(TextReader stdin, TextWriter stdout, TextWriter stderr, params string[] args)
    {
        stdout.WriteLine(string.Join(" ", args));
    }
}