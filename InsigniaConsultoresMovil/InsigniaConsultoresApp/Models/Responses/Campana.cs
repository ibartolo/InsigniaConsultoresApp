using System.Buffers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace InsigniaConsultoresApp.Models.Responses
{
    /// <summary>
    /// Valor tolerante para <c>estatus</c>/<c>estado</c>: la API puede enviarlo como
    /// número o como texto. Conserva ambas representaciones para poder mapear a una
    /// etiqueta en español y, si no se reconoce, caer al valor crudo.
    /// </summary>
    public readonly struct EstatusValue
    {
        /// <summary>Código numérico del estatus (si la API lo envió como número o texto numérico).</summary>
        public int? Code { get; }

        /// <summary>Valor crudo tal cual lo envió la API (texto no numérico o representación original).</summary>
        public string? Raw { get; }

        public EstatusValue(int? code, string? raw)
        {
            Code = code;
            Raw = raw;
        }

        public override string ToString() => Raw ?? Code?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Conversor JSON que acepta número o cadena para <see cref="EstatusValue"/> y
    /// nunca lanza ante un tipo inesperado.
    /// </summary>
    public sealed class EstatusValueJsonConverter : JsonConverter<EstatusValue>
    {
        public override EstatusValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Number:
                    return reader.TryGetInt32(out var code)
                        ? new EstatusValue(code, null)
                        : new EstatusValue(null, ReadRawText(ref reader));

                case JsonTokenType.String:
                    var raw = reader.GetString();
                    return int.TryParse(raw, out var parsed)
                        ? new EstatusValue(parsed, raw)
                        : new EstatusValue(null, raw);

                default:
                    // Null, objeto u otro token inesperado: sin valor.
                    return new EstatusValue(null, null);
            }
        }

        public override void Write(Utf8JsonWriter writer, EstatusValue value, JsonSerializerOptions options)
        {
            if (value.Code.HasValue)
                writer.WriteNumberValue(value.Code.Value);
            else if (value.Raw is not null)
                writer.WriteStringValue(value.Raw);
            else
                writer.WriteNullValue();
        }

        /// <summary>Texto crudo del token actual (p. ej. un número no entero).</summary>
        private static string ReadRawText(ref Utf8JsonReader reader)
        {
            if (reader.HasValueSequence)
                return Encoding.UTF8.GetString(reader.ValueSequence.ToArray());

            return Encoding.UTF8.GetString(reader.ValueSpan);
        }
    }

    /// <summary>
    /// DTO de campaña (endpoint <c>api/Campana</c>).
    /// </summary>
    public sealed class Campana
    {
        [JsonPropertyName("id")] public int Id { get; set; }

        [JsonPropertyName("nombre")] public string? Nombre { get; set; }

        [JsonPropertyName("descripcion")] public string? Descripcion { get; set; }

        [JsonPropertyName("asunto")] public string? Asunto { get; set; }

        [JsonPropertyName("fechaInicio")] public DateTime? FechaInicio { get; set; }

        [JsonPropertyName("fechaFin")] public DateTime? FechaFin { get; set; }

        [JsonPropertyName("estatus")]
        [JsonConverter(typeof(EstatusValueJsonConverter))]
        public EstatusValue Estatus { get; set; }

        [JsonPropertyName("canal")] public int Canal { get; set; }

        [JsonPropertyName("preparada")] public bool Preparada { get; set; }

        [JsonPropertyName("total")] public int Total { get; set; }

        [JsonPropertyName("enviados")] public int Enviados { get; set; }

        [JsonPropertyName("pendientes")] public int Pendientes { get; set; }

        [JsonPropertyName("excluidos")] public int Excluidos { get; set; }

        [JsonPropertyName("errores")] public int Errores { get; set; }

        [JsonPropertyName("porcentaje")] public double? Porcentaje { get; set; }

        [JsonPropertyName("createdDt")] public DateTime? CreatedDt { get; set; }

        /// <summary>
        /// Fecha de creación con respaldo a <see cref="FechaInicio"/> cuando la API
        /// omite o devuelve <c>createdDt</c> nulo (CW-R1).
        /// </summary>
        public DateTime? FechaCreacion => CreatedDt ?? FechaInicio;

        /// <summary>Etiqueta del estatus en español.</summary>
        public string EstatusTexto => CampaignStatus.GetLabel(Estatus);
    }

    /// <summary>
    /// Mapeo del estatus de campaña a etiqueta en español (es-MX), con fallback al valor crudo.
    /// </summary>
    public static class CampaignStatus
    {
        public static string GetLabel(EstatusValue estatus)
        {
            if (estatus.Code.HasValue)
            {
                return estatus.Code.Value switch
                {
                    0 => "Borrador",
                    1 => "Programada",
                    2 => "En proceso",
                    3 => "Completada",
                    4 => "Completada con errores",
                    5 => "Cancelada",
                    _ => estatus.ToString(),
                };
            }

            // Sin código reconocido: se muestra el valor crudo (texto no numérico de la API).
            return estatus.Raw ?? string.Empty;
        }
    }
}
