using System.Net;
using System.Net.Http;
using System.Text.Json;
using InsigniaConsultoresApp.Models;
using InsigniaConsultoresApp.Models.Requests;
using InsigniaConsultoresApp.Models.Responses;

namespace InsigniaConsultoresApp.Services.Api
{
    /// <summary>
    /// Autenticación OAuth2 contra el endpoint /Token y mapeo de errores a <see cref="LoginResult"/>.
    /// Los fallos (400/401, timeout/red, 5xx, respuesta malformada) son resultados esperados,
    /// no excepciones; el mensaje en español se centraliza en <see cref="AuthMessages"/>.
    /// </summary>
    public sealed class AuthService : IAuthService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        private readonly HttpClient _http;

        public AuthService(HttpClient http) => _http = http;

        public async Task<LoginResult> LoginAsync(TokenRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                using var body = request.ToForm();
                using var response = await _http.PostAsync(AuthConfig.TokenEndpoint, body, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync(cancellationToken);
                    if (string.IsNullOrWhiteSpace(json))
                        return LoginResult.Fail(AuthErrorKind.MalformedResponse, AuthMessages.Get(AuthErrorKind.MalformedResponse));

                    var dto = JsonSerializer.Deserialize<TokenResponse>(json, JsonOptions);
                    if (dto is null || string.IsNullOrWhiteSpace(dto.AccessToken))
                        return LoginResult.Fail(AuthErrorKind.MalformedResponse, AuthMessages.Get(AuthErrorKind.MalformedResponse));

                    var nowUtc = DateTime.UtcNow;
                    var session = new SessionModel
                    {
                        Username = request.Username,
                        TokenType = dto.TokenType,
                        AccessToken = dto.AccessToken,
                        ExpiresAtUtc = TokenExpiry.CalculateUtc(nowUtc, dto.ExpiresIn),
                        LastLoginUtc = nowUtc,
                        IsEncrypted = false,
                    };

                    return LoginResult.Ok(session);
                }

                if (response.StatusCode == HttpStatusCode.BadRequest ||
                    response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return LoginResult.Fail(AuthErrorKind.InvalidCredentials, AuthMessages.Get(AuthErrorKind.InvalidCredentials));
                }

                if ((int)response.StatusCode >= 500)
                    return LoginResult.Fail(AuthErrorKind.Server, AuthMessages.Get(AuthErrorKind.Server));

                // Cualquier otro código no esperado se trata como respuesta inválida.
                return LoginResult.Fail(AuthErrorKind.MalformedResponse, AuthMessages.Get(AuthErrorKind.MalformedResponse));
            }
            catch (TaskCanceledException)
            {
                // Timeout del HttpClient o cancelación del token: sin conexión o servidor lento.
                return LoginResult.Fail(AuthErrorKind.Network, AuthMessages.Get(AuthErrorKind.Network));
            }
            catch (HttpRequestException)
            {
                return LoginResult.Fail(AuthErrorKind.Network, AuthMessages.Get(AuthErrorKind.Network));
            }
            catch (JsonException)
            {
                return LoginResult.Fail(AuthErrorKind.MalformedResponse, AuthMessages.Get(AuthErrorKind.MalformedResponse));
            }
        }
    }
}
