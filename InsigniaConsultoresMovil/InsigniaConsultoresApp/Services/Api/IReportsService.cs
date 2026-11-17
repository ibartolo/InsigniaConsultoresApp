using InsigniaConsultoresApp.Models.Responses;

namespace InsigniaConsultoresApp.Services.Api
{
    /// <summary>
    /// Contrato del servicio de reportes/indicadores (endpoint <c>api/Reports/*</c>).
    /// </summary>
    public interface IReportsService
    {
        /// <summary>Indicadores del negocio para el rango [desde, hasta].</summary>
        Task<CampaignResult<IndicadoresPeriodo>> GetIndicadoresAsync(DateTime desde, DateTime hasta, CancellationToken ct = default);
    }
}
