using System;
using System.Text.Json.Serialization;

namespace InsigniaConsultoresApp.Models.Responses
{
    /// <summary>
    /// DTO de la respuesta <c>GET api/Reports/Indicadores</c>.
    /// Conteos enteros del periodo y rango devuelto por el servidor.
    /// Deserialización tolerante: si la API omite un campo, los conteos quedan en 0
    /// y las fechas en <see cref="DateTime.MinValue"/> (el ViewModel cae al rango seleccionado).
    /// </summary>
    public sealed class IndicadoresPeriodo
    {
        [JsonPropertyName("clientesNuevos")] public int ClientesNuevos { get; set; }

        [JsonPropertyName("polizasNuevas")] public int PolizasNuevas { get; set; }

        [JsonPropertyName("polizasRenovadas")] public int PolizasRenovadas { get; set; }

        [JsonPropertyName("polizasVencidas")] public int PolizasVencidas { get; set; }

        [JsonPropertyName("polizasCanceladas")] public int PolizasCanceladas { get; set; }

        [JsonPropertyName("fechaInicio")] public DateTime FechaInicio { get; set; }

        [JsonPropertyName("fechaFin")] public DateTime FechaFin { get; set; }
    }
}
