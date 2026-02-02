namespace nhom6_backend.Models
{
    /// <summary>
    /// Model for external authentication (Google, Facebook, etc.)
    /// </summary>
    public class ExternalLoginModel
    {
        /// <summary>
        /// Provider name: "Google", "Facebook", etc.
        /// </summary>
        public string Provider { get; set; } = string.Empty;

        /// <summary>
        /// Access token from the provider
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// ID token (optional, used by some providers)
        /// </summary>
        public string? IdToken { get; set; }

        /// <summary>
        /// User email (optional, for backup if not in token)
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// User display name (optional)
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// User photo URL (optional)
        /// </summary>
        public string? PhotoUrl { get; set; }
    }
}
