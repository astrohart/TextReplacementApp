using System.Diagnostics;
namespace TextReplacementApp
{
    /// <summary>
    /// Represents a progress report containing information about the current file
    /// being processed
    /// and the progress percentage.
    /// </summary>
    public class ProgressReport
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:TextReplacementApp.ProgressReport" /> class with the specified
        /// current file and progress percentage.
        /// </summary>
        /// <param name="currentFile">The path of the current file being processed.</param>
        /// <param name="progressPercentage">The progress percentage of the operation.</param>
        public ProgressReport(string currentFile, int progressPercentage)
        {
            CurrentFile = currentFile;
            ProgressPercentage = progressPercentage;
        }

        /// <summary>
        /// Gets the path of the current file being processed.
        /// </summary>
        public string CurrentFile { [DebuggerStepThrough] get; }

        /// <summary>
        /// Gets the progress percentage of the operation.
        /// </summary>
        public int ProgressPercentage { [DebuggerStepThrough] get; }
    }
}