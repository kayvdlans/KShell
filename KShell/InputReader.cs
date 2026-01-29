using System.Text;

namespace KShell;

public sealed class InputReader
{
    private bool _insideDoubleQuotes;
    private bool _insideSingleQuotes;

    public List<PipelineSegment> Read(string input)
    {
        var segments = new List<PipelineSegment>();

        _insideDoubleQuotes = false;
        _insideSingleQuotes = false;

        var tokens = new List<string>();
        var current = new StringBuilder();
        for (var i = 0; i < input.Length; i++)
            switch (input[i])
            {
                case '\'':
                    ReadSingleQuote(current, tokens, input, ref i);
                    break;

                case '\"':
                    ReadDoubleQuote(current, tokens, input, ref i);
                    break;

                case '\\':
                    ReadEscapeCharacter(current, input, ref i);
                    break;

                case '|' when !_insideSingleQuotes && !_insideDoubleQuotes && current.Length == 0:
                    segments.Add(new PipelineSegment { Tokens = [.. tokens] });
                    tokens.Clear();
                    break;

                case ' ' when !_insideSingleQuotes && !_insideDoubleQuotes:
                {
                    if (current.Length > 0)
                        tokens.Add(current.ToString());

                    current.Clear();
                    break;
                }

                default:
                    current.Append(input[i]);
                    break;
            }

        if (current.Length > 0)
            tokens.Add(current.ToString());

        segments.Add(new PipelineSegment { Tokens = [.. tokens] });

        return segments;
    }

    private void ReadEscapeCharacter(StringBuilder builder, string input, ref int index)
    {
        if (_insideSingleQuotes)
        {
            builder.Append(input[index]);
            return;
        }

        var isLastCharacter = index == input.Length - 1;

        if (_insideDoubleQuotes)
            switch (input[index + 1])
            {
                case '"':
                case '\\':
                case '$':
                case '`':
                    builder.Append(input[++index]);
                    return;
                case 'n':
                    builder.Append(input[index]);
                    builder.Append(input[++index]);
                    return;
                default:
                    builder.Append(input[index]);
                    return;
            }

        if (isLastCharacter)
            return;

        builder.Append(input[++index]);
    }

    private void ReadSingleQuote(StringBuilder builder, List<string> output, string input, ref int index)
    {
        if (index + 1 < input.Length && input[index + 1] == '\'')
        {
            index++;
            return;
        }

        if (_insideDoubleQuotes)
        {
            builder.Append(input[index]);
            return;
        }

        _insideSingleQuotes = !_insideSingleQuotes;

        if (_insideSingleQuotes)
            return;

        output.Add(builder.ToString());
        builder.Clear();
    }

    private void ReadDoubleQuote(StringBuilder builder, List<string> output, string input, ref int index)
    {
        if (index + 1 < input.Length && input[index + 1] == '\"')
        {
            index++;
            return;
        }

        if (_insideSingleQuotes)
        {
            builder.Append(input[index]);
            return;
        }

        _insideDoubleQuotes = !_insideDoubleQuotes;

        if (_insideDoubleQuotes || (index + 1 < input.Length && input[index + 1] != ' '))
            return;

        output.Add(builder.ToString());
        builder.Clear();
    }
}