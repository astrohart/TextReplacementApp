namespace TextReplacementShared;

/// <summary>Total number of files, number of processed files and number of replaced files.</summary>
/// <param name="TotalFiles">Total number of files.</param>
/// <param name="CompletedFiles">Number of processed files.</param>
/// <param name="ReplacedFiles">Number of replaced files.</param>
/// <param name="ElapsedMilliseconds">The total elapsed time measured by the current instance, in milliseconds.</param>
public record ReplaceResult(int TotalFiles, int CompletedFiles, int ReplacedFiles, long ElapsedMilliseconds)
{
    public override string ToString() => $"""
        Elapsed time {ElapsedMilliseconds:n0} ms.

        Files Total: {TotalFiles:n0}
          Completed: {CompletedFiles:n0}
           Replaced: {ReplacedFiles:n0}
        """;
}
