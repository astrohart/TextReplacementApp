using Newtonsoft.Json;

namespace TextReplacementApp
{
    /// <summary>
    /// Represents the configuration settings of the application.
    /// </summary>
    /// <remarks>
    /// This class encapsulates the configuration settings of the application,
    /// including the directory path, search text, and replace text.
    /// </remarks>
    public class AppConfig
    {
        /// <summary>
        /// Gets or sets the directory path stored in the configuration.
        /// </summary>
        /// <value>
        /// A <see cref="T:System.String" /> representing the directory path.
        /// </value>
        [JsonProperty("directory_path")]
        public string DirectoryPath { get; set; }

        /// <summary>
        /// Gets or sets the replace text stored in the configuration.
        /// </summary>
        /// <value>
        /// A <see cref="T:System.String" /> representing the replace text.
        /// </value>
        [JsonProperty("replace_with")]
        public string ReplaceWith { get; set; }

        /// <summary>
        /// Gets or sets the search text stored in the configuration.
        /// </summary>
        /// <value>
        /// A <see cref="T:System.String" /> representing the search text.
        /// </value>
        [JsonProperty("find_what")]
        public string FindWhat { get; set; }
    }
}