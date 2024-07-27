namespace TextReplacementApp;

/// <summary>Defines the behavior of the application.</summary>
static class Program
{
    /// <summary>The main entry point for the application.</summary>
    [STAThread]
    static void Main() // To customize application configuration such as set high DPI settings or default font, see https://aka.ms/applicationconfiguration.
    {
        ApplicationConfiguration.Initialize();
        using var mainForm = new MainWindow(); // After 'ApplicationConfiguration.Initialize()'.
        Application.Run(mainForm);
    }
}
