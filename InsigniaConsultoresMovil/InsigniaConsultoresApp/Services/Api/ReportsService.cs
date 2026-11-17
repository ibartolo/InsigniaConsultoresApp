using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using InsigniaConsultoresApp.Models.Responses;
using InsigniaConsultoresApp.Services.Storage;

namespace InsigniaConsultoresApp.Services.Api
{
    /// <summary>
    /// Consumo del endpoint <c>GET api/Reports/Indicadores</c>.
    /// Lee el token desde <see cref="ITokenStore.GetSessionAsync"/> y lo asigna como
    /// <c>Authorization: Bearer</c> en cada request (mismo patrón que <see cref="CampaignService"/>).
    /// Los errores esperados (401, red/timeout, 5xx, respuesta malformada) se devuelven
    /// como <see cref="CampaignResult{T}"/>, no como excepciones.
    /// </summary>
    public sealed class ReportsService : IReportsService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        private readonly HttpClient _http;
        private readonly ITokenStore _tokenStore;

        public ReportsService(HttpClient http, ITokenStore tokenStore)
        {
            _http = http;
            _tokenStore = tokenStore;
        }

        public async Task<CampaignResult<IndicadoresPeriodo>> GetIndicadoresAsync(DateTime desde, DateTime hasta, CancellationToken ct = default)
        {
            // Formato de fecha invariante: evita que la cultura local altere el query string.
            var path = $"api/Reports/Indicadores?fechaInicio={desde.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)}&fechaFin={hasta.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)}";
            var raw = await SendAndReadAsync(path, ct);
            if (!raw.IsSuccess)
                return CampaignResult<IndicadoresPeriodo>.Fail(raw.Kind, raw.Message);

            if (string.IsNullOrWhiteSpace(raw.Value))
                return CampaignResult<IndicadoresPeriodo>.Malformed();

            try
            {
                var dto = JsonSerializer.Deserialize<IndicadoresPeriodo>(raw.Value, JsonOptions);
                return dto is null
                    ? CampaignResult<IndicadoresPeriodo>.Malformed()
                    : CampaignResult<IndicadoresPeriodo>.Ok(dto);
            }
            catch (JsonException)
            {
                return CampaignResult<IndicadoresPeriodo>.Malformed();
            }
        }

        /// <summary>
        /// Ejecuta un GET con el bearer token vigente y devuelve el cuerpo crudo como
        /// resultado, clasificando los errores de transporte (401, 5xx, red/timeout).
        /// </summary>
        private async Task<CampaignResult<string>> SendAndReadAsync(string path, CancellationToken ct)
        {
            try
            {
                var session = await _tokenStore.GetSessionAsync();
                using var req = new HttpRequestMessage(HttpMethod.Get, path);

                var token = session?.AccessToken;
                if (!string.IsNullOrWhiteSpace(token))
                    req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                using var resp = await _http.SendAsync(req, ct);

                var json = await resp.Content.ReadAsStringAsync(ct);

                if (resp.StatusCode == HttpStatusCode.Unauthorized)
                    return CampaignResult<string>.Unauthorized();

                if ((int)resp.StatusCode >= 500)
                    return CampaignResult<string>.Server();

                if (!resp.IsSuccessStatusCode)
                    return CampaignResult<string>.Malformed();

                return CampaignResult<string>.Ok(json);
            }
            catch (TaskCanceledException)
            {
                // Timeout del HttpClient o cancelación: sin conexión o servidor lento.
                return CampaignResult<string>.Network();
            }
            catch (HttpRequestException)
            {
                return CampaignResult<string>.Network();
            }
            catch (JsonException)
            {
                return CampaignResult<string>.Malformed();
            }
        }
    }
}
