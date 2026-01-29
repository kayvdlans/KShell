using System.IO.Pipelines;
using System.Text;

namespace KShell.Extensions;

public static class PipeExtensions
{
    public static TextReader AsTextReader(this PipeReader reader, Encoding? enc = null)
    {
        enc ??= Encoding.UTF8;
        return new StreamReader(reader.AsStream(), enc, true, 16 * 1024, false);
    }

    public static TextWriter AsTextWriter(this PipeWriter writer, Encoding? enc = null)
    {
        enc ??= Encoding.UTF8;
        return new StreamWriter(writer.AsStream(), enc, 16 * 1024, false)
        {
            AutoFlush = true
        };
    }
}