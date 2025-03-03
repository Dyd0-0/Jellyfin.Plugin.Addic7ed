using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Jellyfin.Plugin.Addic7ed.Configuration;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Subtitles;
using MediaBrowser.Model.Globalization;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Addic7ed.Providers
{
    /// <summary>
    /// Addic7ed subtitle provider.
    /// </summary>
    public class Addic7edSubtitleProvider : ISubtitleProvider, IHasOrder
    {
        private const string BaseUrl = "https://www.addic7ed.com";
        private const string SearchUrl = BaseUrl + "/search.php?search={0}&Submit=Search";
        private const string LoginUrl = BaseUrl + "/dologin.php";
        private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36";

        private readonly ILogger<Addic7edSubtitleProvider> _logger;
        private readonly ILocalizationManager _localizationManager;
        private readonly HttpClient _httpClient;
        private readonly CookieContainer _cookieContainer;
        private bool _isLoggedIn;
        private DateTime _lastLoginAttempt = DateTime.MinValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="Addic7edSubtitleProvider"/> class.
        /// </summary>
        /// <param name="logger">Instance of the <see cref="ILogger"/> interface.</param>
        /// <param name="localizationManager">Instance of the <see cref="ILocalizationManager"/> interface.</param>
        public Addic7edSubtitleProvider(ILogger<Addic7edSubtitleProvider> logger, ILocalizationManager localizationManager)
        {
            _logger = logger;
            _localizationManager = localizationManager;
            _cookieContainer = new CookieContainer();
            
            var handler = new HttpClientHandler
            {
                CookieContainer = _cookieContainer,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            
            _httpClient = new HttpClient(handler);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
            _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml");
            _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
        }

        /// <inheritdoc />
        public string Name => "Addic7ed";

        /// <inheritdoc />
        public IEnumerable<VideoContentType> SupportedMediaTypes =>
            new[] { VideoContentType.Episode };

        /// <inheritdoc />
        public int Order => 2;

        /// <inheritdoc />
        public async Task<SubtitleResponse> GetSubtitles(string id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting subtitle with id {Id}", id);

            var config = Plugin.Instance?.Configuration;
            if (config == null || !config.Enabled)
            {
                _logger.LogWarning("Addic7ed plugin is not enabled");
                return new SubtitleResponse();
            }

            // Ensure we're logged in if credentials are provided
            if (!string.IsNullOrEmpty(config.Username) && !string.IsNullOrEmpty(config.Password))
            {
                await EnsureLoggedIn(config.Username, config.Password, cancellationToken).ConfigureAwait(false);
            }

            try
            {
                // The id format is: downloadUrl|language|name|format
                var parts = id.Split('|');
                if (parts.Length < 4)
                {
                    _logger.LogError("Invalid subtitle id format: {Id}", id);
                    return new SubtitleResponse();
                }

                var downloadUrl = parts[0];
                var language = parts[1];
                var name = parts[2];
                var format = parts[3];

                _logger.LogInformation("Downloading subtitle from {Url}", downloadUrl);

                var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
                request.Headers.Add("Referer", BaseUrl);

                var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
                memoryStream.Position = 0;

                return new SubtitleResponse
                {
                    Format = format,
                    Language = language,
                    Stream = memoryStream
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading subtitle with id {Id}", id);
                return new SubtitleResponse();
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<RemoteSubtitleInfo>> Search(SubtitleSearchRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Searching subtitles for {Name}, season {Season}, episode {Episode}", 
                request.SeriesName, request.ParentIndexNumber, request.IndexNumber);

            var config = Plugin.Instance?.Configuration;
            if (config == null || !config.Enabled)
            {
                _logger.LogWarning("Addic7ed plugin is not enabled");
                return Array.Empty<RemoteSubtitleInfo>();
            }

            // Ensure we're logged in if credentials are provided
            if (!string.IsNullOrEmpty(config.Username) && !string.IsNullOrEmpty(config.Password))
            {
                await EnsureLoggedIn(config.Username, config.Password, cancellationToken).ConfigureAwait(false);
            }

            // Only support TV shows
            if (request.ContentType != VideoContentType.Episode || 
                string.IsNullOrEmpty(request.SeriesName) || 
                !request.ParentIndexNumber.HasValue || 
                !request.IndexNumber.HasValue)
            {
                return Array.Empty<RemoteSubtitleInfo>();
            }

            try
            {
                var seriesName = NormalizeSeriesName(request.SeriesName);
                var searchUrl = string.Format(SearchUrl, HttpUtility.UrlEncode(seriesName));
                
                _logger.LogInformation("Searching Addic7ed with URL: {Url}", searchUrl);
                
                var html = await _httpClient.GetStringAsync(searchUrl, cancellationToken).ConfigureAwait(false);
                
                // Parse search results to find the correct show
                var showUrl = ParseShowUrl(html, seriesName);
                if (string.IsNullOrEmpty(showUrl))
                {
                    _logger.LogInformation("Show not found: {Name}", seriesName);
                    return Array.Empty<RemoteSubtitleInfo>();
                }
                
                // Get the episode page
                var seasonNumber = request.ParentIndexNumber.Value;
                var episodeNumber = request.IndexNumber.Value;
                var episodeUrl = $"{BaseUrl}{showUrl}/season-{seasonNumber}/episode-{episodeNumber}";
                
                _logger.LogInformation("Getting episode page: {Url}", episodeUrl);
                
                html = await _httpClient.GetStringAsync(episodeUrl, cancellationToken).ConfigureAwait(false);
                
                // Parse subtitles from the episode page
                var subtitles = ParseSubtitles(html, config.SubtitleLanguages);
                
                _logger.LogInformation("Found {Count} subtitles", subtitles.Count);
                
                return subtitles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching subtitles");
                return Array.Empty<RemoteSubtitleInfo>();
            }
        }

        private async Task EnsureLoggedIn(string username, string password, CancellationToken cancellationToken)
        {
            // Check if we're already logged in
            if (_isLoggedIn)
            {
                return;
            }

            // Don't try to login too frequently
            if ((DateTime.UtcNow - _lastLoginAttempt).TotalMinutes < 10)
            {
                return;
            }

            _lastLoginAttempt = DateTime.UtcNow;
            _logger.LogInformation("Logging in to Addic7ed with username: {Username}", username);

            try
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("username", username),
                    new KeyValuePair<string, string>("password", password),
                    new KeyValuePair<string, string>("remember", "1"),
                    new KeyValuePair<string, string>("Submit", "Log in")
                });

                var request = new HttpRequestMessage(HttpMethod.Post, LoginUrl)
                {
                    Content = content
                };
                request.Headers.Add("Referer", BaseUrl);

                var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                var responseHtml = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                
                // Check if login was successful
                _isLoggedIn = !responseHtml.Contains("Username or password incorrect");
                
                if (_isLoggedIn)
                {
                    _logger.LogInformation("Successfully logged in to Addic7ed");
                }
                else
                {
                    _logger.LogWarning("Failed to log in to Addic7ed. Username or password incorrect.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging in to Addic7ed");
                _isLoggedIn = false;
            }
        }

        private string NormalizeSeriesName(string name)
        {
            // Remove special characters and normalize spaces
            var normalized = Regex.Replace(name, @"[^\w\s]", " ");
            normalized = Regex.Replace(normalized, @"\s+", " ").Trim();
            
            // Common replacements for better matching
            normalized = normalized.Replace("and", "&");
            
            return normalized;
        }

        private string ParseShowUrl(string html, string seriesName)
        {
            // Look for show links in the search results
            var showRegex = new Regex(@"<a href=""(/show/\d+)"">([^<]+)</a>");
            var matches = showRegex.Matches(html);
            
            foreach (Match match in matches)
            {
                var url = match.Groups[1].Value;
                var title = match.Groups[2].Value;
                
                // Simple string comparison for now
                if (title.Equals(seriesName, StringComparison.OrdinalIgnoreCase) || 
                    title.Contains(seriesName, StringComparison.OrdinalIgnoreCase) ||
                    seriesName.Contains(title, StringComparison.OrdinalIgnoreCase))
                {
                    return url;
                }
            }
            
            return string.Empty;
        }

        private List<RemoteSubtitleInfo> ParseSubtitles(string html, string[] languages)
        {
            var results = new List<RemoteSubtitleInfo>();
            
            // Extract the table with subtitles
            var tableRegex = new Regex(@"<table[^>]*class=""tabel95""[^>]*>(.*?)</table>", RegexOptions.Singleline);
            var tableMatch = tableRegex.Match(html);
            
            if (!tableMatch.Success)
            {
                return results;
            }
            
            var tableHtml = tableMatch.Groups[1].Value;
            
            // Extract rows
            var rowRegex = new Regex(@"<tr[^>]*class=""epeven completed""[^>]*>(.*?)</tr>|<tr[^>]*class=""epodd completed""[^>]*>(.*?)</tr>", RegexOptions.Singleline);
            var rows = rowRegex.Matches(tableHtml);
            
            foreach (Match row in rows)
            {
                var rowHtml = row.Groups[1].Value;
                if (string.IsNullOrEmpty(rowHtml))
                {
                    rowHtml = row.Groups[2].Value;
                }
                
                // Extract language
                var langRegex = new Regex(@"<td[^>]*class=""language""[^>]*>([^<]+)</td>");
                var langMatch = langRegex.Match(rowHtml);
                
                if (!langMatch.Success)
                {
                    continue;
                }
                
                var language = langMatch.Groups[1].Value.Trim();
                var languageCode = GetLanguageCode(language);
                
                // Skip if not in requested languages
                if (languages != null && languages.Length > 0 && !languages.Contains(languageCode))
                {
                    continue;
                }
                
                // Extract version (release name)
                var versionRegex = new Regex(@"<td[^>]*>.*?Version (.*?),.*?</td>", RegexOptions.Singleline);
                var versionMatch = versionRegex.Match(rowHtml);
                
                if (!versionMatch.Success)
                {
                    continue;
                }
                
                var version = versionMatch.Groups[1].Value.Trim();
                
                // Extract download link
                var downloadRegex = new Regex(@"<a[^>]*href=""(/original/[^""]+)""[^>]*>Download</a>");
                var downloadMatch = downloadRegex.Match(rowHtml);
                
                if (!downloadMatch.Success)
                {
                    continue;
                }
                
                var downloadUrl = BaseUrl + downloadMatch.Groups[1].Value;
                
                // Extract completion status
                var completedRegex = new Regex(@"<td[^>]*>.*?Completed.*?</td>", RegexOptions.Singleline);
                var isCompleted = completedRegex.IsMatch(rowHtml);
                
                // Extract hearing impaired status
                var hiRegex = new Regex(@"<td[^>]*><img[^>]*title=""Hearing Impaired""[^>]*></td>");
                var isHearingImpaired = hiRegex.IsMatch(rowHtml);
                
                // Create subtitle info
                var subtitleName = $"{version} - {language}{(isHearingImpaired ? " (HI)" : "")}";
                var id = $"{downloadUrl}|{languageCode}|{subtitleName}|srt";
                
                results.Add(new RemoteSubtitleInfo
                {
                    Id = id,
                    Name = subtitleName,
                    Author = "Addic7ed",
                    CommunityRating = isCompleted ? 5 : 3,
                    ProviderName = Name,
                    Format = "srt",
                    Comment = isHearingImpaired ? "Hearing Impaired" : string.Empty,
                    IsForced = false,
                    IsHashMatch = false,
                    Language = languageCode
                });
            }
            
            return results;
        }

        private string GetLanguageCode(string language)
        {
            // Map Addic7ed language names to ISO 639-2 codes
            return language.ToLower() switch
            {
                "english" => "en",
                "french" => "fr",
                "spanish" => "es",
                "german" => "de",
                "italian" => "it",
                "portuguese" => "pt",
                "dutch" => "nl",
                "russian" => "ru",
                "chinese" => "zh",
                "japanese" => "ja",
                "korean" => "ko",
                "arabic" => "ar",
                "hebrew" => "he",
                "polish" => "pl",
                "turkish" => "tr",
                "swedish" => "sv",
                "danish" => "da",
                "finnish" => "fi",
                "norwegian" => "no",
                "czech" => "cs",
                "greek" => "el",
                "hungarian" => "hu",
                "romanian" => "ro",
                "bulgarian" => "bg",
                "croatian" => "hr",
                "ukrainian" => "uk",
                _ => "en" // Default to English if unknown
            };
        }
    }
}
