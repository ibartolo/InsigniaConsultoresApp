namespace InsigniaConsultoresApp.Models
{
    /// <summary>
    /// Sesión persistida (SecureStorage + SQLite).
    /// </summary>
    public sealed class SessionModel
    {
        public string? Username { get; set; }
        public string? TokenType { get; set; }
        public string? AccessToken { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
        public DateTime LastLoginUtc { get; set; }
        public bool IsEncrypted { get; set; }
    }
}
