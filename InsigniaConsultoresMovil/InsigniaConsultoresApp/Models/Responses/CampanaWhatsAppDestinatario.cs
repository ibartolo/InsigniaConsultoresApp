using System.Text.Json.Serialization;

namespace InsigniaConsultoresApp.Models.Responses
{
    /// <summary>
    /// DTO de destinatario de campaña WhatsApp
    /// (endpoint <c>api/Campana/{id}/WhatsApp/Destinatarios</c>).
    /// </summary>
    public sealed class CampanaWhatsAppDestinatario
    {
        [JsonPropertyName("id")] public int Id { get; set; }

        [JsonPropertyName("campanaID")] public int CampanaID { get; set; }

        [JsonPropertyName("clienteID")] public int? ClienteID { get; set; }

        [JsonPropertyName("nombre")] public string? Nombre { get; set; }

        [JsonPropertyName("telefono")] public string? Telefono { get; set; }

        [JsonPropertyName("estado")]
        [JsonConverter(typeof(EstatusValueJsonConverter))]
        public EstatusValue Estado { get; set; }

        [JsonPropertyName("fechaEnvio")] public DateTime? FechaEnvio { get; set; }

        /// <summary>Etiqueta del estado del destinatario en español (Pendiente/Enviado).</summary>
        public string EstadoTexto => Estado.Code switch
        {
            0 => "Pendiente",
            1 => "Enviado",
            _ => Estado.ToString(),
        };
    }
}
