using System.Diagnostics;

namespace KShell.Utils;

public static class ExecutableUtils
{
    private static readonly string[] EnvPath = Environment.GetEnvironmentVariable("PATH")?.Split(':') ?? [];
    
    public static bool FindExecutableInPath(string arg, out string executablePath)
    {
        foreach (var path in EnvPath)
        {
            var fullPath = Path.Join(path, arg);

            if (!File.Exists(fullPath))
                continue;

            if (!OperatingSystem.IsWindows() && !IsExecutable(fullPath))
                continue;

            executablePath = fullPath;
            return true;
        }

        executablePath = string.Empty;
        return false;
    }

    private static bool IsExecutable(string path)
    {
        if (OperatingSystem.IsWindows())
            throw new NotSupportedException("Windows not supported.");

        var mode = File.GetUnixFileMode(path);
        return mode.HasFlag(UnixFileMode.UserExecute) ||
               mode.HasFlag(UnixFileMode.GroupExecute) ||
               mode.HasFlag(UnixFileMode.OtherExecute);
    }

    public static List<string> FindPossibleExecutablesInPath(string name)
    {
        List<string> output = [];
        output.AddRange(
            from path in EnvPath
            where Directory.Exists(path)
            from file in Directory.EnumerateFiles(path)
            where Path.GetFileName(file).StartsWith(name) && IsExecutable(file)
            select Path.GetFileName(file));

        return output;
    }

    public static bool Exists(string arg)
    {
        return File.Exists(arg) || FindExecutableInPath(arg, out _);
    }

    public static Process? RunExecutable(string path, bool redirectStdin, params string[] args)
    {
        var startInfo = new ProcessStartInfo(path, args)
        {
            RedirectStandardInput = redirectStdin,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        return Exists(path) ? Process.Start(startInfo) : null;
    }
}