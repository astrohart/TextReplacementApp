namespace TextReplacementShared;

/// <summary>Represents a dialog used to display the progress of an operation.</summary>
/// <remarks>
/// The <see cref="ProgressDialog"/> class provides a simple dialog for displaying progress information during long-running operations.<br/>
/// It contains a label to show the current file being processed and a progress bar to indicate the overall progress of the operation.
/// </remarks>
public partial class ProgressDialog : Form
{
    /// <summary>Initializes a new instance of the <see cref="ProgressDialog"/> class.</summary>
    public ProgressDialog() => InitializeComponent();

    /// <summary>Updates the progress of the operation being displayed in the progress dialog.</summary>
    /// <param name="filePath">The path of the current file being processed.</param>
    /// <param name="progressPercentage">The percentage of completion of the operation.</param>
    /// <remarks>
    /// This method updates the text displayed for the current file being processed and adjusts the progress bar to reflect the progress percentage.<br/>
    /// If invoked from a different thread than the one that created the control, it will use <see cref="Invoke"/> to marshal the call to the proper thread.
    /// </remarks>
    public void UpdateProgress(string filePath, int progressPercentage)
    {
        if (InvokeRequired) Invoke(new Action(() => UpdateProgress(filePath, progressPercentage)));
        else
        {
            lblFilePath.Text = filePath;
            progressBar.Value = progressPercentage;
        }
    }

    /// <summary>Raises the <see cref="Form.Load"/> event.</summary>
    /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Text = Application.ProductName;
    }
}
