namespace KShell.Extensions;

public static class PathExtensions
{
    public static string ToPath(this string path)
    {
        return path.StartsWith('~') ? Path.Join(Environment.GetEnvironmentVariable("HOME"), path[1..]) : path;
    }
}