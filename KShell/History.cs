using KShell.Extensions;

namespace KShell;

public static class History
{
    private const string DefaultHistoryFile = "~/.ksh_history";

    private static readonly string HistoryFile =
        (Environment.GetEnvironmentVariable("HISTFILE") ?? DefaultHistoryFile).ToPath();

    private static int _appendStartIndex;

    private static readonly List<string> Entries = [];

    public static IReadOnlyList<string> All => Entries;

    public static void Initialize()
    {
        try
        {
            var dir = Path.GetDirectoryName(HistoryFile);
            if (dir is not null && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var options = new FileStreamOptions
            {
                Access = FileAccess.ReadWrite, Mode = FileMode.CreateNew, Share = FileShare.ReadWrite
            };

            if (!File.Exists(HistoryFile))
                new FileStream(HistoryFile, options).Dispose();

            ReadFromFile(HistoryFile);
        }
        catch (IOException e)
        {
            Console.Error.WriteLine($"warning: failed to load history: {e.Message}");
        }
    }

    public static void SaveOnExit()
    {
        try
        {
            WriteToFile(HistoryFile);
        }
        catch (IOException e)
        {
            Console.Error.WriteLine($"warning: failed to load history: {e.Message}");
        }
    }

    public static void Add(string command)
    {
        Entries.Add(command);
    }

    public static void ReadFromFile(string path)
    {
        using var stream = File.OpenRead(path); //;FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new StreamReader(stream, true);

        while (true)
        {
            var line = reader.ReadLine();
            if (line is null)
                break;

            Entries.Add(line.Trim());
        }

        _appendStartIndex = Entries.Count;
    }

    public static void WriteToFile(string path)
    {
        File.WriteAllLines(path, Entries);
        _appendStartIndex = Entries.Count;
    }

    public static void AppendToFile(string path)
    {
        File.AppendAllLines(path, Entries[_appendStartIndex..]);
        _appendStartIndex = Entries.Count;
    }

    public struct Cursor
    {
        public bool Active { get; set; }
        public string Prefix { get; set; }
        public int Index { get; set; }
    }
}