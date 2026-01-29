namespace KShell;

public readonly record struct Targets()
{
    public List<string> StdOut { get; } = [];
    public List<string> StdErr { get; } = [];

    public bool WriteStdOut => StdOut.Count > 0;
    public bool WriteStdErr => StdErr.Count > 0;
}