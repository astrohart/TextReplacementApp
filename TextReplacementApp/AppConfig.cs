namespace TextReplacementApp;

/// <summary>Represents the configuration settings of the application.</summary>
/// <remarks>
/// This class encapsulates the configuration settings of the application,
/// including the directory path, search text, and replace text.
/// </remarks>
public class AppConfig
{
    /// <summary>Gets or sets the directory path stored in the configuration.</summary>
    /// <value>A <see cref="string"/> representing the directory path.</value>
    [JsonPropertyName("directoryPath")] public string DirectoryPath { get; set; } = string.Empty;

    /// <summary>Gets or sets the search text stored in the configuration.</summary>
    /// <value>A <see cref="string"/> representing the search text.</value>
    [JsonPropertyName("findWhat")] public string FindWhat { get; set; } = string.Empty;

    /// <summary>Gets or sets the replace text stored in the configuration.</summary>
    /// <value>A <see cref="string"/> representing the replace text.</value>
    [JsonPropertyName("replaceWith")] public string ReplaceWith { get; set; } = string.Empty;
}
