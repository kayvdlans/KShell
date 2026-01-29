namespace KShell.Interfaces;

public interface ICommand
{
    void Run(TextReader stdin, TextWriter stdout, TextWriter stderr, params string[] args);
}