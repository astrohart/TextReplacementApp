namespace TextReplacementApp;

/// <summary>Represents the main form of the TextReplacementApp application.</summary>
/// <remarks>
/// The <see cref="MainWindow"/> class provides the user interface for the application,<br/>
/// allowing users to specify the directory where text replacement should occur and the text to search for and replace.<br/>
/// It also initiates the text replacement process and displays progress using a progress dialog.
/// </remarks>
public partial class MainWindow : Form
{
    /// <summary>The application configuration settings.</summary>
    /// <remarks>This field stores the application configuration settings, including the directory path, search text, and replace text.</remarks>
    readonly AppConfig _appConfig;

    /// <summary>The fully-qualified pathname to the application configuration file.</summary>
    /// <remarks>
    /// This field stores the location of the application configuration file on the file system.<br/>
    /// The file is named <c>config.json</c> and is stored in the '<c>%LOCALAPPDATA%\xyLOGIX\File Text Replacer Tool</c>' directory.
    /// </remarks>
    readonly string _configFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "xyLOGIX",
        "File Text Replacer Tool",
        "config.json");

    /// <summary>Initializes a new instance of the <see cref="MainWindow"/> class.</summary>
    public MainWindow()
    {
        InitializeComponent();

        _appConfig = LoadConfig(); // Initialize AppConfig and load settings.
        UpdateTextBoxesFromConfig();
    }

    /// <summary>Updates the text boxes on the main form with the values stored in the application configuration.</summary>
    /// <remarks>
    /// This method retrieves the directory path, search text, and replace text from the <see cref="AppConfig"/> object<br/>
    /// and sets the corresponding text properties of the text boxes on the main form to these values.
    /// </remarks>
    void UpdateTextBoxesFromConfig()
    {
        txtDirectoryPath.Text = _appConfig.DirectoryPath;
        txtSearchText.Text = _appConfig.FindWhat;
        txtReplaceText.Text = _appConfig.ReplaceWith;
    }

    /// <summary>Raises the <see cref="Form.FormClosing"/> event.</summary>
    /// <param name="e">A <see cref="FormClosingEventArgs"/> that contains the event data.</param>
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        SaveConfig(); // Save config when the form is closing.
    }

    /// <summary>Event handler for the <see cref="Control.Click"/> event of the <b>Browse</b> button.</summary>
    /// <param name="sender">The object that triggered the event.</param>
    /// <param name="e">The event arguments.</param>
    /// <remarks>
    /// This method is called when the user clicks the <b>Browse</b> button to select a directory using the folder browser dialog.<br/>
    /// It updates the text of the directory path textbox with the selected directory path.
    /// </remarks>
    void OnClickBrowseButton(object sender, EventArgs e)
    {
        var result = folderBrowserDialog1.ShowDialog(this);
        if (result is DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog1.SelectedPath)) txtDirectoryPath.Text = folderBrowserDialog1.SelectedPath;
    }

    /// <summary>
    /// Event handler for the <see cref="Control.TextChanged"/> event of the <b>Starting Folder</b> text box.<br/>
    /// Updates the <see cref="AppConfig.DirectoryPath"/> property with the trimmed text from the directory path text box.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event data.</param>
    void OnTextChangedDirectoryPath(object sender, EventArgs e) => _appConfig.DirectoryPath = txtDirectoryPath.Text.Trim();

    /// <summary>
    /// Event handler for the <see cref="Control.TextChanged"/> event of the <b>Find What</b> text box.<br/>
    /// Updates the <see cref="AppConfig.FindWhat"/> property with the trimmed text from the directory path text box.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event data.</param>
    void OnTextChangedSearchText(object sender, EventArgs e) => _appConfig.FindWhat = txtSearchText.Text.Trim();

    /// <summary>
    /// Event handler for the <see cref="Control.TextChanged"/> event of the <b>Replace With</b> text box.<br/>
    /// Updates the <see cref="AppConfig.ReplaceWith"/> property with the trimmed text from the directory path text box.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event data.</param>
    void OnTextChangedReplaceText(object sender, EventArgs e) => _appConfig.ReplaceWith = txtReplaceText.Text.Trim();

    /// <summary>Event handler for the <see cref="Control.Click"/> event of the <b>Do It</b> button.</summary>
    /// <param name="sender">The object that triggered the event.</param>
    /// <param name="e">The event arguments.</param>
    /// <remarks>
    /// This method is called when the user clicks the <b>Do It</b> button to initiate the text replacement process.<br/>
    /// It performs validation, creates a progress dialog, starts a background task for text replacement, and displays the progress dialog modally.
    /// </remarks>
    void OnClickDoItButton(object sender, EventArgs e)
    {
        var directoryPath = txtDirectoryPath.Text.Trim();
        var searchText = txtSearchText.Text.Trim();
        var replaceText = txtReplaceText.Text.Trim();

        // Validation.
        var text = string.Empty;
        if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath)) text = "Please select a valid directory.";
        else if (string.IsNullOrEmpty(searchText)) text = "Please type in some text to find.";
        else if (string.IsNullOrEmpty(replaceText)) text = "Please type in some text to replace the found text with.";

        if (!string.IsNullOrEmpty(text))
        {
            _ = MessageBox.Show(text, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        using var progressDialog = new ProgressDialog();

        // Use Progress<T> to report progress.
        var progressReporter = new Progress<ProgressReport>(report
            => progressDialog.UpdateProgress(report.CurrentFile, report.ProgressPercentage));

        // Start a new task for text replacement.
        _ = Task.Run(() =>
            {
                var res = Common.ReplaceTextInFiles(directoryPath, searchText, replaceText, progressReporter, this);

                // Close the progress dialog.
                if (InvokeRequired)
                    _ = progressDialog.BeginInvoke(new MethodInvoker(progressDialog.Close));
                else
                    progressDialog.Close();

                var text = $"""
                    Text replacement completed.

                    Elapsed time {res.ElapsedMilliseconds:n0} ms.

                    Files Total .......: {res.TotalFiles:n0}
                        Completed .: {res.CompletedFiles:n0}
                        Replaced ....: {res.ReplacedFiles:n0}
                    """;

                // Show completion message ('text...' the formatting makes one cry).
                _ = MessageBox.Show(text, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });

        _ = progressDialog.ShowDialog(this);
    }

    /// <summary>Loads the configuration settings from the specified JSON file.</summary>
    /// <remarks>
    /// This method reads the configuration settings from the specified JSON file and deserializes them into an <see cref="AppConfig"/> instance.<br/>
    /// If the file does not exist, a new instance of <see cref="AppConfig"/> is returned with all properties set to empty strings.
    /// </remarks>
    /// <returns>
    /// An instance of <see cref="AppConfig"/> containing the loaded configuration settings.
    /// If the file does not exist, a new instance of <see cref="AppConfig"/> is returned.
    /// </returns>
    AppConfig LoadConfig()
    {
        try // If config file doesn't exist, return a new instance.
        {
            if (File.Exists(_configFilePath))
            {
                var json = File.ReadAllText(_configFilePath); // Load config from JSON file.
                return JsonSerializer.Deserialize<AppConfig>(json) ?? new();
            }
        }
        catch (Exception ex) { Common.DisplayException(ex); } // Display an alert with the exception text.

        return new();
    }

    /// <summary>Saves the current application configuration to a JSON file.</summary>
    /// <remarks>
    /// The method first checks if the directory containing the configuration file exists, and creates it if it does not.<br/>
    /// Then, it serializes the <see cref="AppConfig"/> object to JSON format using <c>System.Text.Json</c>, and writes the JSON string to the configuration file.
    /// </remarks>
    void SaveConfig()
    {
        try // Check for any conditions that might prevent us from succeeding.
        {
            var json = JsonSerializer.Serialize(_appConfig, new JsonSerializerOptions { WriteIndented = true });

            if (!string.IsNullOrWhiteSpace(json) && Path.GetDirectoryName(_configFilePath) is string directory)
            {
                _ = Directory.CreateDirectory(directory);
                File.WriteAllText(_configFilePath, json);
            }
        }
        catch (Exception ex) { Common.DisplayException(ex); } // Display an alert with the exception text.
    }
}
