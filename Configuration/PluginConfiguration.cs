using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.Addic7ed.Configuration
{
    /// <summary>
    /// Plugin configuration class for Addic7ed subtitle provider.
    /// </summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
        /// </summary>
        public PluginConfiguration()
        {
            Username = string.Empty;
            Password = string.Empty;
            MaxSubtitleDownloadsPerDay = 40; // Addic7ed limit
            SubtitleLanguages = new string[] { "en" };
            Enabled = true;
        }

        /// <summary>
        /// Gets or sets the Addic7ed username.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the Addic7ed password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the provider is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of subtitle downloads per day.
        /// </summary>
        public int MaxSubtitleDownloadsPerDay { get; set; }

        /// <summary>
        /// Gets or sets the subtitle languages to download.
        /// </summary>
        public string[] SubtitleLanguages { get; set; }
    }
}