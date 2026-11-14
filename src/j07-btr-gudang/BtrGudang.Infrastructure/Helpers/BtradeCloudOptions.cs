using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BtrGudang.Infrastructure.Helpers
{
    /// <summary>
    /// Configuration for the BTrade cloud API.
    /// Bound from the "BtradeCloud" section of AppSettings.json.
    /// </summary>
    public class BtradeCloudOptions
    {
        public const string SECTION_NAME = "BtradeCloud";

        /// <summary>
        /// Base URL used when the configuration does not specify one.
        /// </summary>
        public const string DEFAULT_BASE_URL = "https://dev.smart-ics.com:8089/belajar-api";

        /// <summary>
        /// Cloud API base URL, for example
        /// "https://dev.smart-ics.com:8089/belajar-api".
        /// </summary>
        public string BaseUrl { get; set; } = DEFAULT_BASE_URL;

        /// <summary>
        /// Returns the configured base URL, falling back to
        /// <see cref="DEFAULT_BASE_URL"/> when it is blank. Trailing slashes are
        /// removed so callers can safely append "/api/..." without producing a
        /// double slash.
        /// </summary>
        public string GetEffectiveBaseUrl()
        {
            var baseUrl = string.IsNullOrWhiteSpace(BaseUrl) ? DEFAULT_BASE_URL : BaseUrl;
            return baseUrl.Trim().TrimEnd('/');
        }
    }
}
