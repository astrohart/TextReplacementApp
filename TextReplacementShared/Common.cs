#define UseArray

namespace TextReplacementShared;

public sealed class Common
{
    Common() { /* Utility classes should not have public constructors. */ }

    /* Account for this algorithm being run on a Visual Studio solution consisting only of C# projects, and in a local Git repo. */

    static readonly string[] ExclusionsArr = [".git", ".vs", ".vscode", "bin", "obj", "packages"]; // Consider the order...
    static readonly string[] InclusionsArr = [".config", ".cs", ".csproj", ".json", ".md", ".resx", ".settings", ".txt"]; // Consider the order...

    static readonly SearchValues<string> Exclusions = SearchValues.Create(ExclusionsArr, StringComparison.OrdinalIgnoreCase);
    static readonly SearchValues<string> Inclusions = SearchValues.Create(InclusionsArr, StringComparison.OrdinalIgnoreCase);

    static IWin32Window? s_owner; // Determine whether invoked by a Forms application or a Console application.

    /// <summary>Replaces text in all files within the specified directory and its subdirectories.</summary>
    /// <param name="directoryPath">The path of the directory to search for files.</param>
    /// <param name="searchText">The text to search for in each file.</param>
    /// <param name="replaceText">The text to replace the search text with.</param>
    /// <param name="progressReporter">An object for reporting progress during text replacement.</param>
    /// <param name="owner">An implementation of IWin32Window that will own the modal dialog box.</param>
    /// <remarks>
    /// This method recursively searches for files within the specified directory and its subdirectories.<br/>
    /// For each file found, it calls <see cref="ReplaceTextInFile"/> to perform text replacement.<br/>
    /// Progress is reported using the specified <paramref name="progressReporter"/>.<br/>
    /// Certain directories (e.g., <c>.git</c>, <c>.vs</c>, etc.) are excluded from text replacement.
    /// </remarks>
    /// <returns><see cref="ReplaceResult"/>(TotalFiles, CompletedFiles, ReplacedFiles) - Total number of files, number of processed files and number of replaced files.</returns>
    public static ReplaceResult ReplaceTextInFiles(in string directoryPath, in string searchText, in string replaceText, IProgress<ProgressReport>? progressReporter = null, IWin32Window? owner = null)
    {
        var sw = Stopwatch.StartNew();
        var totalFiles = 0d; // (double) for 'progressPercentage'.
        var completedFiles = 0;
        var replacedFiles = 0;
        var minLength = searchText.Length; // If the original file length is less than 'searchText', skip the file.

        try
        {
            s_owner = owner;

            var files = new FileSystemEnumerable<string>(
                directory: directoryPath,
                transform: (ref FileSystemEntry entry) => entry.ToSpecifiedFullPath(),
                options: new EnumerationOptions { AttributesToSkip = FileAttributes.None, RecurseSubdirectories = true })
            {
                ShouldRecursePredicate = (ref FileSystemEntry entry) => !entry.Attributes.HasFlag(FileAttributes.ReparsePoint) && !Exclusions.Contains(entry.FileName.ToString()),
                ShouldIncludePredicate = (ref FileSystemEntry entry) => !entry.IsDirectory && Inclusions.Contains(Path.GetExtension(entry.FileName).ToString()) && entry.Length >= minLength
            }
            .ToList(); // For 'totalFiles'.

            totalFiles = files.Count;

            foreach (var file in files)
            {
                if (ReplaceTextInFile(file, searchText, replaceText, skipCheck: true)) _ = Interlocked.Increment(ref replacedFiles);
                _ = Interlocked.Increment(ref completedFiles);

                // Report progress (if not 'null').
                progressReporter?.Report(new ProgressReport(file, (int)(completedFiles / totalFiles * 100)));
            }
        }
        catch (Exception ex) { DisplayException(ex); } // Display an alert with the exception text.

        return new((int)totalFiles, completedFiles, replacedFiles, sw.ElapsedMilliseconds); // Finally..
    }

    /// <summary>Replaces text in the specified file.</summary>
    /// <param name="filePath">The path of the file to perform text replacement on.</param>
    /// <param name="searchText">The text to search for in the file.</param>
    /// <param name="replaceText">The text to replace the search text with.</param>
    /// <param name="skipCheck">Defaults to <b>false</b> - Performs some validation of directory and extension.</param>
    /// <remarks>
    /// This method performs text replacement in the specified file. It reads the content of the file,<br/>
    /// replaces occurrences of the search text with the replace text, and writes the modified content back to the file.<br/>
    /// If <paramref name="skipCheck"/> is <b>false</b> check if the file path contains specific directories (such as <c>.git</c>, <c>.vs</c>, etc.), in that case it skips the replacement.
    /// </remarks>
    /// <returns>Returns <b>true</b> if the file has been processed, else <b>false</b>.</returns>
    public static bool ReplaceTextInFile(in string filePath, in string searchText, in string replaceText, bool skipCheck = false)
    {
        try
        {
            if (!skipCheck && SkipFile(filePath)) return false;  // Don't like the naming but want default to be false...

            using var fileStream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

            var originalLength = fileStream.Length;
            if (!skipCheck && originalLength < searchText.Length) return false; // If the original file length is less than 'searchText', return early.

            var originalText = ReadTextFromMemory(fileStream, originalLength); // Read the content from memory.
            if (string.IsNullOrWhiteSpace(originalText)) return false;

            var modifiedText = originalText.Replace(searchText, replaceText, StringComparison.Ordinal); // Perform text replacement.

            if (modifiedText.Equals(originalText, StringComparison.Ordinal)) return false; // File not modified.
            // Debug.WriteLine($"CHANGED - {filePath}");

            var modifiedLength = searchText.Length == replaceText.Length
                ? originalLength
                : Encoding.UTF8.GetByteCount(modifiedText); // Calculate the length of the modified text.

            if (modifiedLength != originalLength) // The modified text has changed the length, set the new file size.
            {
                fileStream.SetLength(modifiedLength);
                _ = fileStream.Seek(0, SeekOrigin.Begin); // Re-open the file stream after changing the size.
            }

            return WriteTextToMemory(fileStream, modifiedLength, modifiedText); // Write the modified content back to memory.
        }
        catch (Exception ex) { DisplayException(ex); return false; } // Display an alert with the exception text.

#if !UseArray
        static bool SkipFile(string filePath)
            => !InclusionsArr.Contains(Path.GetExtension(filePath), StringComparer.OrdinalIgnoreCase) || // Cheapest...
                Array.Exists(ExclusionsArr, x => filePath.Contains($@"\{x}\", StringComparison.OrdinalIgnoreCase));
#else
        static bool SkipFile(in string filePath)
            => !Inclusions.Contains(Path.GetExtension(filePath)) || // Cheapest...
                filePath.Split('\\').SkipLast(1).Any(static x => Exclusions.Contains(x)); // Hmm.. not pleasant.
#endif
    }

    /// <summary>Reads text from a memory-mapped file accessor.</summary>
    /// <param name="fileStream">An open <see cref="FileStream"/> to memory-map.</param>
    /// <param name="length">The length of the text to read.</param>
    /// <remarks>
    /// This method reads text from a memory-mapped file accessor and returns it as a string.<br/>
    /// It reads the specified length of bytes from the accessor and decodes them using UTF-8 encoding.
    /// </remarks>
    /// <returns>The text read from memory.</returns>
    static string ReadTextFromMemory(FileStream fileStream, long length)
    {
        try // Check for conditions that would prohibit our success.
        {
            using var mmf = MemoryMappedFile.CreateFromFile(fileStream, mapName: null, length, MemoryMappedFileAccess.Read, HandleInheritability.None, leaveOpen: true);
            using var accessor = mmf.CreateViewAccessor(0, length, MemoryMappedFileAccess.Read);

            // if (!accessor.CanRead || length <= 0) return string.Empty; // Redundant...

            var bytes = new byte[length];
            _ = accessor.ReadArray(0, bytes, 0, (int)length);

            return Encoding.UTF8.GetString(bytes); // Decode UTF8.
        }
        catch (Exception ex) { DisplayException(ex); return string.Empty; } // Display an alert with the exception text.
    }

    /// <summary>Writes the specified text to a memory-mapped view accessor.</summary>
    /// <param name="fileStream">The <see cref="FileStream"/> to memory-map.</param>
    /// <param name="length">The length of the text to write.</param>
    /// <param name="text">The text to write.</param>
    /// <remarks>
    /// This method writes the UTF-8 encoded bytes of the <paramref name="text"/> to the memory-mapped view accessor
    /// starting at the beginning (offset zero).<br/>It is the responsibility of the caller to ensure that the
    /// length of the text matches the length of the memory-mapped view accessor.
    /// </remarks>
    static bool WriteTextToMemory(FileStream fileStream, long length, in string text)
    {
        try // Check for conditions that would prohibit our success.
        {
            // if (fileStream is null || string.IsNullOrWhiteSpace(text)) return false; // Rely on exception with a message, what if text really is empty...

            using var mmf = MemoryMappedFile.CreateFromFile(fileStream, mapName: null, length, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, leaveOpen: false);
            using var accessor = mmf.CreateViewAccessor(0, length, MemoryMappedFileAccess.ReadWrite);

            var bytes = Encoding.UTF8.GetBytes(text);
            accessor.WriteArray(0, bytes, 0, bytes.Length);

            return true;
        }
        catch (Exception ex) { DisplayException(ex); return false; } // Display an alert with the exception text.
    }

    public static void DisplayException(Exception ex)
    {
        if (s_owner is null) Console.WriteLine($"\n**ERROR: {ex.Message}\n"); // Write the exception information to the console.
        else
            _ = MessageBox.Show(s_owner, ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
    }
}
