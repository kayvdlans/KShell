using System.Text;
using KShell.Utils;

namespace KShell;

public static class Autocomplete
{
    public static void PrintMatches(StringBuilder input, ConsoleKey previousStroke)
    {
        List<string> matches = [];

        var currentInput = input.ToString();
        var lastWordIdx = currentInput.LastIndexOf(' ') + 1;
        var lastWord = currentInput[lastWordIdx..];

        var completionMode = GetCompletionMode(lastWordIdx, lastWord);

        if (completionMode == CompletionMode.PathExecutables)
        {
            matches.AddRange(CommandRunner.GetPossibleCommands(lastWord));

            if (matches.Count == 0)
                matches.AddRange(
                    ExecutableUtils.FindPossibleExecutablesInPath(lastWord));
        }
        else
        {
            matches.AddRange(FileUtils.GetFileSystemEntries(lastWord));
        }

        matches.Sort();

        switch (matches.Count)
        {
            case 0:
                Console.Write('\a');
                break;
            case 1:
                foreach (var ch in matches[0][lastWord.Length..])
                {
                    input.Append(ch);
                    Console.Write(ch);
                }

                input.Append(' ');
                Console.Write(' ');
                break;

            default:
                var commonPrefix = matches[0];
                for (var i = 1; i < matches.Count; i++)
                    commonPrefix = GetLongestCommonPrefix(commonPrefix, matches[i]);

                if (commonPrefix.Length > lastWord.Length)
                {
                    var suffix = commonPrefix[lastWord.Length..];
                    input.Append(suffix);
                    Console.Write(suffix);
                    break;
                }

                if (previousStroke != ConsoleKey.Tab)
                {
                    Console.Write('\a');
                    break;
                }

                Console.Write('\n');
                for (var i = 0; i < matches.Count; i++)
                {
                    Console.Write(matches[i]);
                    if (i != matches.Count - 1)
                        Console.Write("  ");
                    else
                        Console.Write('\n');
                }

                Console.Write($"$ {input}");

                break;
        }
    }

    private static CompletionMode GetCompletionMode(int tokenStart, string token)
    {
        if (token.Contains('/'))
            return CompletionMode.FileSystem;

        return tokenStart == 0 ? CompletionMode.PathExecutables : CompletionMode.FileSystem;
    }

    private static string GetLongestCommonPrefix(string a, string b)
    {
        var len = Math.Min(a.Length, b.Length);
        var i = 0;
        while (i < len && a[i] == b[i])
            i++;
        return a[..i];
    }

    private enum CompletionMode
    {
        PathExecutables,
        FileSystem
    }
}