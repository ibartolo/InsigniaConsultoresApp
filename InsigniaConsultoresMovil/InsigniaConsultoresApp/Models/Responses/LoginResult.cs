using InsigniaConsultoresApp.Models;

namespace InsigniaConsultoresApp.Models.Responses
{
    /// <summary>
    /// Tipos de error de autenticación mapeados a mensajes en español.
    /// </summary>
    public enum AuthErrorKind
    {
        None,
        InvalidCredentials,
        Network,
        Server,
        MalformedResponse,
        Storage,
    }

    /// <summary>
    /// Resultado discriminado del login para la UI.
    /// </summary>
    public sealed class LoginResult
    {
        public bool Success { get; private init; }
        public SessionModel? Session { get; private init; }
        public AuthErrorKind Error { get; private init; }
        public string Message { get; private init; } = "";

        public static LoginResult Ok(SessionModel session) =>
            new() { Success = true, Session = session };

        public static LoginResult Fail(AuthErrorKind error, string message) =>
            new() { Error = error, Message = message };
    }

    /// <summary>
    /// Mensajes de error de autenticación en español.
    /// </summary>
    public static class AuthMessages
    {
        public static string Get(AuthErrorKind error) => error switch
        {
            AuthErrorKind.InvalidCredentials => "Usuario o contraseña incorrectos",
            AuthErrorKind.Network => "No se pudo conectar con el servidor",
            AuthErrorKind.Server => "Ocurrió un error en el servidor, intenta de nuevo",
            AuthErrorKind.MalformedResponse => "Respuesta inválida del servidor",
            AuthErrorKind.Storage => "No se pudo guardar la sesión",
            _ => "Ocurrió un error",
        };
    }
}
