namespace InsigniaConsultoresApp.Services.Api
{
    /// <summary>
    /// Configuración del backend OAuth2 y parámetros de autenticación.
    /// </summary>
    public static class AuthConfig
    {
        /// <summary>URL base del API.</summary>
        public const string BaseUrl = "https://webapi.insigniaconsultores.com.mx";

        /// <summary>Endpoint OAuth2 para obtener el token.</summary>
        public const string TokenEndpoint = BaseUrl + "/Token";

        /// <summary>Margen de seguridad para la expiración del token (20 min = 1200 s).</summary>
        public const int TokenExpiryMarginSeconds = 20 * 60;

        /// <summary>Timeout de las solicitudes HTTP (30 s). Se aplica al HttpClient en el wiring de DI.</summary>
        public const int RequestTimeoutSeconds = 30;
    }
}
