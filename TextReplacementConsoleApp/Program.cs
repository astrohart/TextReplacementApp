using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading;
using Directory = Alphaleonis.Win32.Filesystem.Directory;
using File = Alphaleonis.Win32.Filesystem.File;
using Path = Alphaleonis.Win32.Filesystem.Path;

namespace TextReplacementConsoleApp
{
    public static class ListExtensions
    {
        /// <summary>
        /// Compares the <paramref name="value" /> object with the
        /// <paramref name="testObjects" /> provided, to see if any of the
        /// <paramref name="testObjects" /> is a match.
        /// </summary>
        /// <typeparam name="T"> Type of the object to be tested. </typeparam>
        /// <param name="value"> Source object to check. </param>
        /// <param name="testObjects">
        /// Object or objects that should be compared to value
        /// with the <see cref="M:System.Object.Equals" /> method.
        /// </param>
        /// <returns>
        /// True if any of the <paramref name="testObjects" /> equals the value;
        /// false otherwise.
        /// </returns>
        public static bool IsAnyOf<T>(this T value, params T[] testObjects)
            => testObjects.Contains(value);
    }

    /// <summary>
    /// Defines the behaviors of the application.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The entry point of the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Console.Title = "Text Replacement Console App";

            const string directoryPath =
                @"C:\Users\Brian Hart\source\repos\astrohart\NuGetPackageAutoUpdater";
            const string searchText = "Foo";
            const string replaceText = "Bar";

            Console.WriteLine($"Searching all code in '{directoryPath}'...");
            Console.WriteLine(
                $"Replacing '{searchText}' with '{replaceText}'..."
            );

            Console.WriteLine($"Start Time: {DateTime.Now:O}");

            ReplaceTextInFiles(directoryPath, searchText, replaceText);

            Console.WriteLine($"End Time: {DateTime.Now:O}");
            Console.WriteLine("Text replacement completed.");

            Console.ReadKey();
        }

        /// <summary>
        /// Reads text from a memory-mapped file accessor.
        /// </summary>
        /// <param name="accessor">The memory-mapped file accessor.</param>
        /// <param name="length">The length of the text to read.</param>
        /// <returns>The text read from memory.</returns>
        /// <remarks>
        /// This method reads text from a memory-mapped file accessor and returns it as a
        /// string.
        /// It reads the specified length of bytes from the accessor and decodes them using
        /// UTF-8 encoding.
        /// </remarks>
        private static string ReadTextFromMemory(
            UnmanagedMemoryAccessor accessor,
            long length
        )
        {
            // text contents of the file.
            var result = string.Empty;

            try
            {
                // check for conditions that would prohibit our success
                if (accessor == null) return result;
                if (!accessor.CanRead) return result;
                if (length <= 0L) return result;

                var bytes = new byte[length];
                accessor.ReadArray(0, bytes, 0, (int)length);
                result = Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                // If an exception was caught for any reason, then return the empty string
                result = string.Empty;
            }

            return result;
        }

        /// <summary>
        /// Replaces text in the specified file.
        /// </summary>
        /// <param name="filePath">The path of the file to perform text replacement on.</param>
        /// <param name="searchText">The text to search for in the file.</param>
        /// <param name="replaceText">The text to replace the search text with.</param>
        /// <remarks>
        /// This method performs text replacement in the specified file. It reads
        /// the content of the file, replaces occurrences of the search text with the
        /// replace text, and writes the modified content back to the file. If the file
        /// path contains specific directories (such as <c>.git</c>, <c>.vs</c>, etc.), it
        /// skips the replacement.
        /// </remarks>
        private static void ReplaceTextInFile(
            string filePath,
            string searchText,
            string replaceText
        )
        {
            /*
             * Account for this algorithm being run on a
             * Visual Studio solution consisting only of
             * C# projects, and in a local Git repo.
             */

            if (filePath.Contains(@"\.git\")) return;
            if (filePath.Contains(@"\.vs\")) return;
            if (filePath.Contains(@"\packages\")) return;
            if (filePath.Contains(@"\bin\")) return;
            if (filePath.Contains(@"\obj\")) return;
            if (!Path.GetExtension(filePath)
                     .IsAnyOf(
                         ".txt", ".cs", ".resx", ".config", ".json", ".csproj",
                         ".settings", ".md"
                     ))
                return;

            using (var fileStream = File.Open(
                       filePath, FileMode.Open, FileAccess.ReadWrite,
                       FileShare.None
                   ))
            {
                var originalLength = fileStream.Length;

                // If the original file length is zero, return early
                if (originalLength == 0) return;

                using (var mmf = MemoryMappedFile.CreateFromFile(
                           fileStream, null, originalLength,
                           MemoryMappedFileAccess.ReadWrite,
                           HandleInheritability.None, false
                       ))
                {
                    using (var accessor = mmf.CreateViewAccessor(
                               0, originalLength,
                               MemoryMappedFileAccess.ReadWrite
                           ))
                    {
                        // Read the content from memory.  If no text was obtained, stop.
                        var text = ReadTextFromMemory(accessor, originalLength);
                        if (string.IsNullOrWhiteSpace(text))
                            return;

                        // Perform text replacement
                        text = text.Replace(searchText, replaceText);

                        // Calculate the length of the modified text
                        long modifiedLength = Encoding.UTF8.GetByteCount(text);

                        // If the modified text is larger, extend the file size
                        if (modifiedLength > originalLength)
                        {
                            fileStream.SetLength(modifiedLength);

                            // Re-open the file stream after extending the size
                            fileStream.Seek(0, SeekOrigin.Begin);
                            using (var newMmf = MemoryMappedFile.CreateFromFile(
                                       fileStream, null, modifiedLength,
                                       MemoryMappedFileAccess.ReadWrite,
                                       HandleInheritability.None, false
                                   ))
                            {
                                using (var newAccessor =
                                       newMmf.CreateViewAccessor(
                                           0, modifiedLength,
                                           MemoryMappedFileAccess.ReadWrite
                                       ))

                                    // Write the modified content back to memory
                                    WriteTextToMemory(newAccessor, text);
                            }
                        }
                        else
                        {
                            // Write the modified content back to memory
                            WriteTextToMemory(accessor, text);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Replaces text in all files within the specified directory and its
        /// subdirectories.
        /// </summary>
        /// <param name="directoryPath">The path of the directory to search for files.</param>
        /// <param name="searchText">The text to search for in each file.</param>
        /// <param name="replaceText">The text to replace the search text with.</param>
        /// <remarks>
        /// This method recursively searches for files within the specified
        /// directory and its subdirectories. For each file found, it calls
        /// <see cref="ReplaceTextInFile" /> to perform text replacement. Certain
        /// directories (e.g., <c>.git</c>, <c>.vs</c>, etc.) are excluded from text
        /// replacement.
        /// </remarks>
        private static void ReplaceTextInFiles(
            string directoryPath,
            string searchText,
            string replaceText
        )
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    Console.WriteLine(
                        $"ERROR: The folder '{directoryPath}' was not found on the file system."
                    );
                    return;
                }

                var files = Directory.EnumerateFiles(
                                         directoryPath, "*",
                                         SearchOption.AllDirectories
                                     )
                                     .Where(
                                         file => !file.Contains(@"\.git\") &&
                                                 !file.Contains(@"\.vs\") &&
                                                 !file.Contains(
                                                     @"\packages\"
                                                 ) &&
                                                 !file.Contains(@"\bin\") &&
                                                 !file.Contains(@"\obj\")
                                     )
                                     .ToList();

                var completedFiles = 0;

                foreach (var file in files)
                {
                    ReplaceTextInFile(file, searchText, replaceText);
                    Interlocked.Increment(ref completedFiles);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
            }
        }

        /// <summary>
        /// Writes the specified text to a memory-mapped view accessor.
        /// </summary>
        /// <param name="accessor">The memory-mapped view accessor to write to.</param>
        /// <param name="text">The text to write.</param>
        /// <remarks>
        /// This method writes the UTF-8 encoded bytes of the
        /// <paramref name="text" /> to the memory-mapped view accessor starting at the
        /// beginning (offset zero). It is the responsibility of the caller to ensure that
        /// the
        /// length of the text matches the length of the memory-mapped view accessor.
        /// </remarks>
        private static void WriteTextToMemory(
            UnmanagedMemoryAccessor accessor,
            string text
        )
        {
            try
            {
                // check for conditions that would prohibit our success
                if (accessor == null) return;
                if (!accessor.CanWrite) return;
                if (string.IsNullOrWhiteSpace(text)) return;

                var bytes = Encoding.UTF8.GetBytes(text);
                accessor.WriteArray(0, bytes, 0, bytes.Length);
            }
            catch (Exception ex)
            {
                // write the exception information to the console
                Console.WriteLine($"ERROR: {ex.Message}");
            }
        }
    }
}