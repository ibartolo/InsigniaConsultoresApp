namespace InsigniaConsultoresApp.Models.Responses
{
    /// <summary>
    /// Tipos de error de las llamadas a campañas, mapeados a mensajes en español.
    /// </summary>
    public enum CampaignErrorKind
    {
        None,
        Unauthorized,
        Network,
        Server,
        Malformed,
    }

    /// <summary>
    /// Resultado discriminado de una operación de campañas sin payload
    /// (p. ej. Preparar, Marcar enviado/pendiente). Espejo de <see cref="LoginResult"/>:
    /// los errores esperados son resultados, no excepciones.
    /// </summary>
    public sealed class CampaignResult
    {
        public bool IsSuccess { get; private init; }
        public CampaignErrorKind Kind { get; private init; }
        public string? Message { get; private init; }

        public static CampaignResult Ok() => new() { IsSuccess = true };

        public static CampaignResult Unauthorized(string? message = null) => Fail(CampaignErrorKind.Unauthorized, message);

        public static CampaignResult Network(string? message = null) => Fail(CampaignErrorKind.Network, message);

        public static CampaignResult Server(string? message = null) => Fail(CampaignErrorKind.Server, message);

        public static CampaignResult Malformed(string? message = null) => Fail(CampaignErrorKind.Malformed, message);

        public static CampaignResult Fail(CampaignErrorKind kind, string? message = null) =>
            new() { Kind = kind, Message = message ?? CampaignMessages.Get(kind) };
    }

    /// <summary>
    /// Resultado discriminado de una operación de campañas con payload.
    /// </summary>
    public sealed class CampaignResult<T>
    {
        public bool IsSuccess { get; private init; }
        public T? Value { get; private init; }
        public CampaignErrorKind Kind { get; private init; }
        public string? Message { get; private init; }

        public static CampaignResult<T> Ok(T value) => new() { IsSuccess = true, Value = value };

        public static CampaignResult<T> Unauthorized(string? message = null) => Fail(CampaignErrorKind.Unauthorized, message);

        public static CampaignResult<T> Network(string? message = null) => Fail(CampaignErrorKind.Network, message);

        public static CampaignResult<T> Server(string? message = null) => Fail(CampaignErrorKind.Server, message);

        public static CampaignResult<T> Malformed(string? message = null) => Fail(CampaignErrorKind.Malformed, message);

        public static CampaignResult<T> Fail(CampaignErrorKind kind, string? message = null) =>
            new() { Kind = kind, Message = message ?? CampaignMessages.Get(kind) };
    }

    /// <summary>
    /// Mensajes de error de campañas en español (es-MX).
    /// </summary>
    public static class CampaignMessages
    {
        public static string Get(CampaignErrorKind error) => error switch
        {
            CampaignErrorKind.Unauthorized => "Tu sesión ha expirado, inicia sesión de nuevo",
            CampaignErrorKind.Network => "No se pudo conectar con el servidor",
            CampaignErrorKind.Server => "Ocurrió un error en el servidor, intenta de nuevo",
            CampaignErrorKind.Malformed => "Respuesta inválida del servidor",
            _ => "Ocurrió un error",
        };
    }
}
