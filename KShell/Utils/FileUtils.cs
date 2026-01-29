namespace KShell.Utils;

public static class FileUtils
{
    public static void WriteToFile(string path, string content)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllText(path, content);
    }

    public static void AppendToFile(string path, string content)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        File.AppendAllText(path, content);
    }

    public static List<string> GetFileSystemEntries(string token)
    {
        token = ReplaceTilde(token);

        string prefix;
        string dir;
        if (token.Contains('/'))
        {
            var lastIdx = token.LastIndexOf('/');
            prefix = token[(lastIdx + 1)..];
            dir = lastIdx == 0 ? "/" : token[..lastIdx];
        }
        else
        {
            dir = ".";
            prefix = token;
        }

        if (!Directory.Exists(dir))
            return [];

        List<string> output = [];
        output.AddRange(
            from entry in Directory.EnumerateFileSystemEntries(dir)
            let name = Path.GetFileName(entry)
            where name.StartsWith(prefix)
            let isDir = Directory.Exists(entry)
            select (dir == "." ? name : $"{dir}/{name}") + (isDir ? "/" : "")
        );

        output.Sort();
        return output;
    }

    private static string ReplaceTilde(string token)
    {
        if (!token.StartsWith('~'))
            return token;

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (token == "~")
            return home;
        if (token.StartsWith("~/"))
            return home + token[1..];

        return token;
    }
}