Console.Title = "Text Replacement Console App";

var directoryPath = args.Length > 0 ? args[0] : @"c:\temp";
var searchText = args.Length > 1 ? args[1] : "FooBarFoo";
var replaceText = args.Length > 2 ? args[2] : "FooBarFoo-1";

Console.WriteLine($"""
    Searching all code in '{directoryPath}'.
    Replacing '{searchText}' with '{replaceText}' ...
    Start Time ..: {DateTime.UtcNow.ToLocalTime()}
    """);

var res = Common.ReplaceTextInFiles(directoryPath, searchText, replaceText);

Console.WriteLine($"""
    End Time ....: {DateTime.UtcNow.ToLocalTime()}
    Text replacement completed.
    {res}
    """);

// Console.ReadKey();

#if true
{
    Console.WriteLine("\n/* Simple test of 'ReplaceTextInFile()'. */\n"); // <==

    if (!Directory.Exists(directoryPath)) return;

    var files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
    var processed = 0;

    foreach (var file in files.Where(file => Common.ReplaceTextInFile(file, searchText, replaceText)))
    {
        Console.WriteLine($"Processed: '{file}'.");
        processed++;
    }

    Console.WriteLine($"""
        Files Total: {files.Length:n0}
          Processed: {processed:n0}

        Replaced == Processed ('{res.ReplacedFiles == processed}')
        """);
}
#endif

#if true
{
    Console.WriteLine("\n/* Simple test of 'ProgressDialog()'. */\n"); // <==

    using var progressDialog = new ProgressDialog();

    // Use Progress<T> to report progress.
    var progressReporter = new Progress<ProgressReport>(report
        => progressDialog.UpdateProgress(report.CurrentFile, report.ProgressPercentage));

    // Start a new task for text replacement.
    _ = Task.Run(() =>
    {
        _ = Common.ReplaceTextInFiles(directoryPath, searchText, replaceText, progressReporter);

        // Close the progress dialog.
        // if (InvokeRequired)
        _ = progressDialog.BeginInvoke(new MethodInvoker(progressDialog.Close));
        // else progressDialog.Close();

        // Show completion message.
        // _ = MessageBox.Show("Text replacement completed.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
    });

    _ = progressDialog.ShowDialog();
}
#endif

#if false
{
    Console.WriteLine("\n/* Simple test of 'ReplaceTextInFile()'. */\n"); // <==

    var files = Directory.EnumerateFiles(@"c:\temp", "olle.txt", SearchOption.AllDirectories).ToArray();
    var processed = 0;

    foreach (var file in files.Where(file => Common.ReplaceTextInFile(file, "kalle", "pelle", skipCheck: true)))
    {
        Console.WriteLine($"Processed: '{file}'.");
        processed++;
    }

    Console.WriteLine($"""
        Files Total: {files.Length:n0}
          Processed: {processed:n0}
        """);
}
#endif
