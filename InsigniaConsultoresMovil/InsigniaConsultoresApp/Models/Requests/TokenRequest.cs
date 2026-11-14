using System.Net.Http;

namespace InsigniaConsultoresApp.Models.Requests
{
    /// <summary>
    /// Datos de login enviados al endpoint OAuth2 (grant_type=password).
    /// </summary>
    public sealed class TokenRequest
    {
        public string Username { get; init; } = "";
        public string Password { get; init; } = "";
        public string GrantType { get; init; } = "password";

        /// <summary>
        /// Construye el body url-encoded (application/x-www-form-urlencoded),
        /// lo que codifica caracteres especiales como los de la contraseña.
        /// </summary>
        public FormUrlEncodedContent ToForm() => new(new Dictionary<string, string>
        {
            ["grant_type"] = GrantType,
            ["username"] = Username,
            ["password"] = Password,
        });
    }
}
