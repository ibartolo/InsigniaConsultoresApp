using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace InsigniaConsultoresApp.Models.Responses
{
    /// <summary>
    /// Envoltura estándar de respuesta del backend (ModelResponse).
    /// Tolerante al payload: admite que venga bajo la clave <c>enum</c> o <c>response</c>,
    /// o ausente; los campos desconocidos se capturan sin lanzar excepciones.
    /// </summary>
    public sealed class ModelResponseDto
    {
        [JsonPropertyName("isSuccess")] public bool IsSuccess { get; set; }

        [JsonPropertyName("message")] public string? Message { get; set; }

        /// <summary>Campos desconocidos (incluido <c>enum</c>/<c>response</c>), capturados sin fallar.</summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }

        /// <summary>Payload de la respuesta: <c>enum</c> o <c>response</c>, en ese orden.</summary>
        public string? Payload
        {
            get
            {
                var data = ExtensionData;
                if (data is null)
                    return null;

                var key = FindKey(data, "enum") ?? FindKey(data, "response");
                return key is null ? null : ElementToString(data[key]);
            }
        }

        private static string? FindKey(Dictionary<string, JsonElement> data, string name)
        {
            foreach (var key in data.Keys)
            {
                if (string.Equals(key, name, System.StringComparison.OrdinalIgnoreCase))
                    return key;
            }

            return null;
        }

        private static string? ElementToString(JsonElement element) => element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            _ => element.GetRawText(),
        };
    }
}
