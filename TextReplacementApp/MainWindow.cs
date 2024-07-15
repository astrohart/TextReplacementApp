using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Directory = Alphaleonis.Win32.Filesystem.Directory;
using File = Alphaleonis.Win32.Filesystem.File;
using Path = Alphaleonis.Win32.Filesystem.Path;

namespace TextReplacementApp
{
    /// <summary>
    /// Represents the main form of the TextReplacementApp application.
    /// </summary>
    /// <remarks>
    /// The <see cref="T:TextReplacementApp.MainWindow" /> class provides the
    /// user interface for the application, allowing users to specify the directory
    /// where text replacement should occur and the text to search for and replace. It
    /// also initiates the text replacement process and displays progress using a
    /// progress dialog.
    /// </remarks>
    public partial class MainWindow : Form
    {
        /// <summary>
        /// The application configuration settings.
        /// </summary>
        /// <remarks>
        /// This field stores the application configuration settings,
        /// including the directory path, search text, and replace text.
        /// </remarks>
        private readonly AppConfig appConfig;

        /// <summary>
        /// The fully-qualified pathname to the application configuration file.
        /// </summary>
        /// <remarks>
        /// This field stores the location of the application configuration file
        /// on the file system. The file is named <c>config.json</c> and is stored in the
        /// <c>%LOCALAPPDATA%\xyLOGIX\File Text Replacer Tool</c> directory.
        /// </remarks>
        private readonly string configFilePath = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData
            ), "xyLOGIX, LLC", "File Text Replacer Tool", "config.json"
        );

        /// <summary>
        /// Constructs a new instance of <see cref="T:TextReplacementApp.MainWindow" /> and
        /// returns a reference to it.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Initialize AppConfig and load settings
            appConfig = LoadConfig();
            UpdateTextBoxesFromConfig();
        }

        /// <summary>
        /// Gets a reference to an instance of a collection, each of whose elements are of
        /// type <see cref="T:TextReplacementApp.FileFailureInfo" />, that individually,
        /// describe the file(s) for which I/O operation(s) failed and the reason(s) for
        /// those failures.
        /// </summary>
        private IList<FileFailureInfo> FileFailures { get; } =
            new List<FileFailureInfo>();

        /// <summary>
        /// Gets or sets the value of the <b>Find What</b> text box.
        /// </summary>
        public string FindWhat
        {
            [DebuggerStepThrough] get => findWhatTextBox.Text;
            [DebuggerStepThrough] set => findWhatTextBox.Text = value;
        }

        /// <summary>
        /// Gets or sets the value of the <b>Replace With</b> text box.
        /// </summary>
        public string ReplaceWith
        {
            [DebuggerStepThrough] get => replaceWithTextBox.Text;
            [DebuggerStepThrough] set => replaceWithTextBox.Text = value;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing" />
        /// event.
        /// </summary>
        /// <param name="e">
        /// A <see cref="T:System.Windows.Forms.FormClosingEventArgs" />
        /// that contains the event data.
        /// </param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // Save config when the form is closing
            SaveConfig();
        }

        /// <summary>
        /// Loads the configuration settings from the specified JSON file.
        /// </summary>
        /// <returns>
        /// An instance of <see cref="T:TextReplacementApp.AppConfig" />
        /// containing the loaded configuration settings. If the file does not exist, a new
        /// instance of <see cref="T:TextReplacementApp.AppConfig" /> is returned.
        /// </returns>
        /// <remarks>
        /// This method reads the configuration settings from the specified JSON
        /// file and deserializes them into an
        /// <see cref="T:TextReplacementApp.AppConfig" />
        /// instance. If the file does not exist, a new instance of
        /// <see cref="T:TextReplacementApp.AppConfig" /> is returned with all properties
        /// set to empty strings.
        /// </remarks>
        private AppConfig LoadConfig()
        {
            AppConfig result;

            try
            {
                if (!File.Exists(configFilePath))

                    // If config file doesn't exist, return a new instance
                    return new AppConfig();

                // Load config from JSON file
                var json = File.ReadAllText(configFilePath);
                result = JsonConvert.DeserializeObject<AppConfig>(json);
            }
            catch (Exception ex)
            {
                // display an alert with the exception text
                MessageBox.Show(
                    this, ex.Message, Application.ProductName,
                    MessageBoxButtons.OK, MessageBoxIcon.Stop
                );

                result = default;
            }

            return result;
        }

        /// <summary>
        /// Event handler for the <see cref="E:System.Windows.Forms.Control.Click" /> event
        /// event of the <b>Browse</b> button.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">The event arguments.</param>
        /// <remarks>
        /// This method is called when the user clicks the <b>Browse</b> button to
        /// select a directory using the folder browser dialog. It updates the text of the
        /// directory path textbox with the selected directory path.
        /// </remarks>
        private void OnClickBrowseButton(object sender, EventArgs e)
        {
            var result = folderBrowserDialog1.ShowDialog(this);
            if (result == DialogResult.OK &&
                !string.IsNullOrWhiteSpace(folderBrowserDialog1.SelectedPath))
                txtDirectoryPath.Text = folderBrowserDialog1.SelectedPath;
        }

        /// <summary>
        /// Event handler for the <see cref="E:System.Windows.Forms.Control.Click" /> event
        /// of the <b>Do It</b> button.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">The event arguments.</param>
        /// <remarks>
        /// This method is called when the user clicks the <b>Do It</b> button
        /// to initiate the text replacement process. It performs validation,
        /// creates a progress dialog, starts a background task for text replacement,
        /// and displays the progress dialog modally.
        /// </remarks>
        private void OnClickDoItButton(object sender, EventArgs e)
        {
            var directoryPath = txtDirectoryPath.Text.Trim();
            var searchText = findWhatTextBox.Text.Trim();
            var replaceText = replaceWithTextBox.Text.Trim();

            // validation
            if (string.IsNullOrEmpty(directoryPath) ||
                !Directory.Exists(directoryPath))
            {
                MessageBox.Show(
                    "Please select a valid directory.", Application.ProductName,
                    MessageBoxButtons.OK, MessageBoxIcon.Error
                );
                return;
            }

            if (string.IsNullOrEmpty(searchText))
            {
                MessageBox.Show(
                    "Please type in some text to find.",
                    Application.ProductName, MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            if (string.IsNullOrEmpty(replaceText))
            {
                MessageBox.Show(
                    "Please type in some text to replace the found text with.",
                    Application.ProductName, MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            using (var progressDialog = new ProgressDialog())
            {
                // Use Progress<T> to report progress
                var progressReporter = new Progress<ProgressReport>(
                    report =>
                    {
                        progressDialog.UpdateProgress(
                            report.CurrentFile, report.ProgressPercentage
                        );
                    }
                );

                // Start a new task for text replacement
                Task.Run(
                    () =>
                    {
                        ReplaceTextInFiles(
                            directoryPath, searchText, replaceText,
                            progressReporter
                        );

                        // close the progress dialog
                        if (InvokeRequired)
                            progressDialog.BeginInvoke(
                                new MethodInvoker(progressDialog.Close)
                            );
                        else
                            progressDialog.Close();

                        // Show completion message - unless there are more than zero failures
                        if (FileFailures.Count <= 0)
                            MessageBox.Show(
                                "Text replacement completed.", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information
                            );
                    }
                );

                progressDialog.ShowDialog(this);
            }
        }

        /// <summary>
        /// Handles the <see cref="E:System.Windows.Forms.Control.Click" /> event raised by
        /// the <b>Switch</b> button when it is clicked by the user.
        /// </summary>
        /// <param name="sender">
        /// Reference to an instance of the object that raised the
        /// event.
        /// </param>
        /// <param name="e">
        /// A <see cref="T:System.EventArgs" /> that contains the event
        /// data.
        /// </param>
        /// <remarks>
        /// This method responds by juxtaposing the values of the <b>Find What</b>
        /// and <b>Replace With</b> text boxes.
        /// </remarks>
        private void OnClickSwitchButton(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FindWhat) &&
                string.IsNullOrWhiteSpace(ReplaceWith))
                return; // nothing to do

            (ReplaceWith, FindWhat) = (FindWhat, ReplaceWith);
        }

        /// <summary>
        /// Event handler for the
        /// <see cref="E:System.Windows.Forms.Control.TextChanged" /> event of the
        /// <b>Starting Folder</b> text box. Updates the
        /// <see cref="P:TextReplacementApp.AppConfig.DirectoryPath" /> property with the
        /// trimmed text from the directory path text box.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void OnTextChangedDirectoryPath(object sender, EventArgs e)
            => appConfig.DirectoryPath = txtDirectoryPath.Text.Trim();

        /// <summary>
        /// Event handler for the
        /// <see cref="E:System.Windows.Forms.Control.TextChanged" /> event of the
        /// <b>Replace With</b> text box. Updates the
        /// <see cref="P:TextReplacementApp.AppConfig.ReplaceWith" /> property with the
        /// trimmed text from the directory path text box.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void OnTextChangedReplaceText(object sender, EventArgs e)
            => appConfig.ReplaceWith = replaceWithTextBox.Text.Trim();

        /// <summary>
        /// Event handler for the
        /// <see cref="E:System.Windows.Forms.Control.TextChanged" /> event of the
        /// <b>Find What</b> text box. Updates the
        /// <see cref="P:TextReplacementApp.AppConfig.FindWhat" /> property with the
        /// trimmed text from the directory path text box.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void OnTextChangedSearchText(object sender, EventArgs e)
            => appConfig.FindWhat = findWhatTextBox.Text.Trim();

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
        private void ReplaceTextInFile(
            string filePath,
            string searchText,
            string replaceText
        )
        {
            try
            {
                /*
                 * Account for this algorithm being run on a
                 * Visual Studio solution consisting only of
                 * C# projects, and in a local Git repo.
                 */

                if (string.IsNullOrWhiteSpace(filePath)) return;

                if (filePath.Contains(@"\.git\")) return;
                if (filePath.Contains(@"\.vs\")) return;
                if (filePath.Contains(@"\packages\")) return;
                if (filePath.Contains(@"\bin\")) return;
                if (filePath.Contains(@"\obj\")) return;
                if (!Path.GetExtension(filePath)
                         .IsAnyOf(
                             ".txt", ".cs", ".resx", ".config", ".json",
                             ".csproj", ".settings", ".md"
                         ))
                    return;

                if (!File.Exists(filePath)) return;

                var text = string.Empty;
                var originalLength = 0L;

                using (var reader = new FileStreamReader(filePath))
                {
                    originalLength = reader.Length;

                    text = reader.ReadAllText();
                }

                if (!text.Contains(searchText)) return;

                // Perform text replacement
                var newText = text.Replace(searchText, replaceText).Trim();

                // Calculate the length of the modified text
                long modifiedLength = Encoding.UTF8.GetByteCount(newText);
                
                // No sense in writing out the replacement text if no replacement actually took place
                if (newText.Equals(text) &&
                    modifiedLength.Equals(originalLength)) return;

                using (var writer = new FileStreamWriter(filePath))
                {
                    writer.SetLength(modifiedLength);

                    writer.WriteAllText(newText);
                }
            }
            catch (Exception ex)
            {
                if (typeof(UnauthorizedAccessException) != ex.GetType())
                    FileFailures.Add(new FileFailureInfo(filePath, ex));
            }
        }

        /// <summary>
        /// Replaces text in all files within the specified directory and its
        /// subdirectories.
        /// </summary>
        /// <param name="directoryPath">The path of the directory to search for files.</param>
        /// <param name="searchText">The text to search for in each file.</param>
        /// <param name="replaceText">The text to replace the search text with.</param>
        /// <param name="progressReporter">
        /// An object for reporting progress during text
        /// replacement.
        /// </param>
        /// <remarks>
        /// This method recursively searches for files within the specified
        /// directory and its subdirectories. For each file found, it calls
        /// <see cref="ReplaceTextInFile" /> to perform text replacement. Progress is
        /// reported using the specified <paramref name="progressReporter" />. Certain
        /// directories (e.g., <c>.git</c>, <c>.vs</c>, etc.) are excluded from text
        /// replacement.
        /// </remarks>
        private void ReplaceTextInFiles(
            string directoryPath,
            string searchText,
            string replaceText,
            IProgress<ProgressReport> progressReporter
        )
        {
            var files = Directory.EnumerateFiles(
                                     directoryPath, "*",
                                     SearchOption.AllDirectories
                                 )
                                 .Where(
                                     file => !file.Contains(@"\.git\") &&
                                             !file.Contains(@"\.vs\") &&
                                             !file.Contains(@"\packages\") &&
                                             !file.Contains(@"\bin\") &&
                                             !file.Contains(@"\obj\")
                                 )
                                 .ToList();

            var totalFiles = files.Count;
            var completedFiles = 0;

            foreach (var file in files)
            {
                ReplaceTextInFile(file, searchText, replaceText);
                Interlocked.Increment(ref completedFiles);

                // Report progress
                var progressPercentage =
                    (int)((double)completedFiles / totalFiles * 100);
                var progressReport = new ProgressReport(
                    file, progressPercentage
                );
                progressReporter.Report(progressReport);
            }
        }

        /// <summary>
        /// Saves the current application configuration to a JSON file.
        /// </summary>
        /// <remarks>
        /// The method first checks if the directory containing the configuration
        /// file exists, and creates it if it does not. Then, it serializes the
        /// <see cref="T:TextReplacementApp.AppConfig" /> object to JSON format using
        /// <c>Newtonsoft.Json</c>, and writes the JSON string to the configuration file.
        /// </remarks>
        private void SaveConfig()
        {
            try
            {
                // check for any conditions that might prevent us from succeeding.
                if (string.IsNullOrWhiteSpace(configFilePath)) return;

                var directory = Path.GetDirectoryName(configFilePath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var json = JsonConvert.SerializeObject(
                    appConfig, Formatting.Indented
                );
                if (string.IsNullOrWhiteSpace(json)) return;

                File.WriteAllText(configFilePath, json);
            }
            catch (Exception ex)
            {
                // display an alert with the exception text
                MessageBox.Show(
                    this, ex.Message, Application.ProductName,
                    MessageBoxButtons.OK, MessageBoxIcon.Stop
                );
            }
        }

        /// <summary>
        /// Updates the text boxes on the main form with the values stored in the
        /// application configuration.
        /// </summary>
        /// <remarks>
        /// This method retrieves the directory path, search text, and replace
        /// text from the <see cref="T:TextReplacementApp.AppConfig" /> object and sets the
        /// corresponding text properties of the text boxes on the main form to these
        /// values.
        /// </remarks>
        private void UpdateTextBoxesFromConfig()
        {
            txtDirectoryPath.Text = appConfig.DirectoryPath;
            findWhatTextBox.Text = appConfig.FindWhat;
            replaceWithTextBox.Text = appConfig.ReplaceWith;
        }
    }
}