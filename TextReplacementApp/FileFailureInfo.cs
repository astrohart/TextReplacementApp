using System;

namespace TextReplacementApp
{
    /// <summary>
    /// Encapsulates information about a file that experienced an I/O operation
    /// failure.
    /// </summary>
    public class FileFailureInfo
    {
        /// <summary>
        /// Constructs a new instance of
        /// <see cref="T:TextReplacementApp.FileFailureInfo" /> and returns a reference to
        /// it.
        /// </summary>
        public FileFailureInfo(string filePath, Exception exception)
        {
            FilePath = filePath;
            Exception = exception;
        }

        /// <summary>
        /// Gets the <see cref="T:System.Exception" /> that occurred.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Gets a <see cref="T:System.String" /> containing the fully-qualified pathname
        /// of the file that experienced a failure.
        /// </summary>
        public string FilePath { get; }
    }
}