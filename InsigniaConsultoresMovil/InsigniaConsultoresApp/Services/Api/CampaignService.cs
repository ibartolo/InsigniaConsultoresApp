using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using InsigniaConsultoresApp.Models.Responses;
using InsigniaConsultoresApp.Services.Storage;

namespace InsigniaConsultoresApp.Services.Api
{
    /// <summary>
    /// Consumo de los endpoints <c>api/Campana/*</c>.
    /// Lee el token desde <see cref="ITokenStore.GetSessionAsync"/> y lo asigna como
    /// <c>Authorization: Bearer</c> en cada request (sin DelegatingHandler, para no
    /// afectar el <see cref="HttpClient"/> compartido con el login).
    /// Los errores esperados (401, red/timeout, 5xx, respuesta malformada) se devuelven
    /// como <see cref="CampaignResult"/>, no como excepciones.
    /// </summary>
    public sealed class CampaignService : ICampaignService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        private readonly HttpClient _http;
        private readonly ITokenStore _tokenStore;

        public CampaignService(HttpClient http, ITokenStore tokenStore)
        {
            _http = http;
            _tokenStore = tokenStore;
        }

        public async Task<CampaignResult<List<Campana>>> ListWhatsAppAsync(CancellationToken ct = default)
        {
            var raw = await SendAndReadAsync(HttpMethod.Get, "api/Campana/List", ct);
            if (!raw.IsSuccess)
                return CampaignResult<List<Campana>>.Fail(raw.Kind, raw.Message);

            // Sin cuerpo se interpreta como lista vacía, no como error.
            if (string.IsNullOrWhiteSpace(raw.Value))
                return CampaignResult<List<Campana>>.Ok(new List<Campana>());

            try
            {
                var list = JsonSerializer.Deserialize<List<Campana>>(raw.Value, JsonOptions);
                if (list is null)
                    return CampaignResult<List<Campana>>.Malformed();

                // Solo campañas WhatsApp (Canal == 2).
                var whatsApp = list.Where(c => c.Canal == 2).ToList();
                return CampaignResult<List<Campana>>.Ok(whatsApp);
            }
            catch (JsonException)
            {
                return CampaignResult<List<Campana>>.Malformed();
            }
        }

        public async Task<CampaignResult<Campana>> GetAsync(int id, CancellationToken ct = default)
        {
            var raw = await SendAndReadAsync(HttpMethod.Get, $"api/Campana/{id}", ct);
            if (!raw.IsSuccess)
                return CampaignResult<Campana>.Fail(raw.Kind, raw.Message);

            if (string.IsNullOrWhiteSpace(raw.Value))
                return CampaignResult<Campana>.Malformed();

            try
            {
                var dto = JsonSerializer.Deserialize<Campana>(raw.Value, JsonOptions);
                return dto is null
                    ? CampaignResult<Campana>.Malformed()
                    : CampaignResult<Campana>.Ok(dto);
            }
            catch (JsonException)
            {
                return CampaignResult<Campana>.Malformed();
            }
        }

        public async Task<CampaignResult> PrepareRecipientsAsync(int campaignId, CancellationToken ct = default)
            => await PostAsync($"api/Campana/{campaignId}/WhatsApp/Preparar", ct);

        public async Task<CampaignResult<List<CampanaWhatsAppDestinatario>>> GetRecipientsAsync(int campaignId, CancellationToken ct = default)
        {
            var raw = await SendAndReadAsync(HttpMethod.Get, $"api/Campana/{campaignId}/WhatsApp/Destinatarios", ct);
            if (!raw.IsSuccess)
                return CampaignResult<List<CampanaWhatsAppDestinatario>>.Fail(raw.Kind, raw.Message);

            // Sin cuerpo: todavía no hay destinatarios (no es un error).
            if (string.IsNullOrWhiteSpace(raw.Value))
                return CampaignResult<List<CampanaWhatsAppDestinatario>>.Ok(new List<CampanaWhatsAppDestinatario>());

            try
            {
                using var doc = JsonDocument.Parse(raw.Value);

                // El API puede devolver un arreglo directo o una envoltura con el arreglo dentro.
                var array = FindArray(doc.RootElement);
                if (array is null)
                {
                    Debug.WriteLine($"[CampaignService] Destinatarios con forma inesperada: {Truncate(raw.Value)}");
                    return CampaignResult<List<CampanaWhatsAppDestinatario>>.Malformed();
                }

                var list = array.Value.Deserialize<List<CampanaWhatsAppDestinatario>>(JsonOptions);
                return CampaignResult<List<CampanaWhatsAppDestinatario>>.Ok(list ?? new List<CampanaWhatsAppDestinatario>());
            }
            catch (JsonException)
            {
                Debug.WriteLine($"[CampaignService] Destinatarios no deserializables: {Truncate(raw.Value)}");
                return CampaignResult<List<CampanaWhatsAppDestinatario>>.Malformed();
            }
        }

        /// <summary>
        /// Busca el arreglo de datos en la respuesta: lo devuelve tal cual si la raíz ya es
        /// un arreglo, o el primer arreglo que encuentre dentro de la envoltura.
        /// </summary>
        private static JsonElement? FindArray(JsonElement root)
        {
            if (root.ValueKind == JsonValueKind.Array)
                return root;

            if (root.ValueKind != JsonValueKind.Object)
                return null;

            foreach (var name in new[] { "data", "response", "result", "items", "destinatarios" })
            {
                if (root.TryGetProperty(name, out var el) && el.ValueKind == JsonValueKind.Array)
                    return el;
            }

            foreach (var prop in root.EnumerateObject())
            {
                if (prop.Value.ValueKind == JsonValueKind.Array)
                    return prop.Value;
            }

            return null;
        }

        private static string Truncate(string value)
            => value.Length <= 300 ? value : value.Substring(0, 300) + "...";

        public async Task<CampaignResult> MarkSentAsync(int recipientId, CancellationToken ct = default)
            => await PostAsync($"api/Campana/WhatsApp/Enviado/{recipientId}", ct);

        public async Task<CampaignResult> MarkPendingAsync(int recipientId, CancellationToken ct = default)
            => await PostAsync($"api/Campana/WhatsApp/Pendiente/{recipientId}", ct);

        /// <summary>
        /// Construye un request con el bearer token vigente leído del almacén de sesión.
        /// </summary>
        private async Task<HttpRequestMessage> CreateRequestAsync(HttpMethod method, string path)
        {
            var session = await _tokenStore.GetSessionAsync();
            var req = new HttpRequestMessage(method, path);

            var token = session?.AccessToken;
            if (!string.IsNullOrWhiteSpace(token))
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return req;
        }

        /// <summary>
        /// Ejecuta un GET y devuelve el cuerpo crudo como resultado, clasificando los
        /// errores de transporte (401, 5xx, red/timeout, respuesta no esperada).
        /// </summary>
        private async Task<CampaignResult<string>> SendAndReadAsync(HttpMethod method, string path, CancellationToken ct)
        {
            try
            {
                using var req = await CreateRequestAsync(method, path);
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

        private static bool TryGetProperty(JsonElement obj, string name, out JsonElement value)
        {
            if (obj.TryGetProperty(name, out value))
                return true;

            foreach (var prop in obj.EnumerateObject())
            {
                if (string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    value = prop.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Ejecuta un POST sin payload; el cuerpo de éxito es una envoltura
        /// <see cref="ModelResponseDto"/>. Si <c>isSuccess</c> es <c>false</c> se
        /// clasifica como <see cref="CampaignErrorKind.Server"/> con el mensaje del API.
        /// </summary>
        private async Task<CampaignResult> PostAsync(string path, CancellationToken ct)
        {
            try
            {
                using var req = await CreateRequestAsync(HttpMethod.Post, path);
                using var resp = await _http.SendAsync(req, ct);

                if (resp.StatusCode == HttpStatusCode.Unauthorized)
                    return CampaignResult.Unauthorized();

                var json = await resp.Content.ReadAsStringAsync(ct);

                if ((int)resp.StatusCode >= 500)
                    return CampaignResult.Server();

                if (!resp.IsSuccessStatusCode)
                    return CampaignResult.Malformed($"Respuesta inválida del servidor ({(int)resp.StatusCode})");

                // 2xx: el API puede responder vacío, un arreglo o la envoltura {isSuccess,...}.
                // Solo se considera fallo si la envoltura declara explícitamente isSuccess=false.
                if (string.IsNullOrWhiteSpace(json))
                    return CampaignResult.Ok();

                try
                {
                    using var doc = JsonDocument.Parse(json);

                    if (doc.RootElement.ValueKind == JsonValueKind.Object &&
                        TryGetProperty(doc.RootElement, "isSuccess", out var flag) &&
                        flag.ValueKind != JsonValueKind.True)
                    {
                        string? message = null;
                        if (TryGetProperty(doc.RootElement, "message", out var m) && m.ValueKind == JsonValueKind.String)
                            message = m.GetString();

                        return CampaignResult.Server(message);
                    }

                    return CampaignResult.Ok();
                }
                catch (JsonException)
                {
                    // Cuerpo no-JSON con 2xx: la operación se dio por buena.
                    return CampaignResult.Ok();
                }
            }
            catch (TaskCanceledException)
            {
                return CampaignResult.Network();
            }
            catch (HttpRequestException)
            {
                return CampaignResult.Network();
            }
            catch (JsonException)
            {
                return CampaignResult.Malformed();
            }
        }
    }
}
