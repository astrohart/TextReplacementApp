namespace TextReplacementShared;

/// <summary>Represents a progress report containing information about the current file being processed and the progress percentage.</summary>
/// <remarks>Initializes a new instance of the <see cref="ProgressReport"/> class with the specified current file and progress percentage.</remarks>
/// <param name="currentFile">The path of the current file being processed.</param>
/// <param name="progressPercentage">The progress percentage of the operation.</param>
public class ProgressReport(string currentFile, int progressPercentage)
{
    /// <summary>Gets the path of the current file being processed.</summary>
    public string CurrentFile { get; } = $"{(currentFile.Length <= 59 ? currentFile : $"{currentFile[..29]}…{currentFile[^29..]}")}";
    // public string CurrentFile { get; } = currentFile;

    /// <summary>Gets the progress percentage of the operation.</summary>
    public int ProgressPercentage { get; } = progressPercentage;
}
