namespace KShell;

public sealed class PipelineSegment
{
    public string[] Tokens { get; set; } = [];
    public Targets RedirectTargets { get; } = new();
    public Targets AppendTargets { get; } = new();
}