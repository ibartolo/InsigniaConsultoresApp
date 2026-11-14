using System.Collections.Generic;
using InsigniaConsultoresApp.Models.Responses;

namespace InsigniaConsultoresApp.Services.Api
{
    /// <summary>
    /// Contrato del servicio de campañas WhatsApp (endpoints <c>api/Campana/*</c>).
    /// </summary>
    public interface ICampaignService
    {
        /// <summary>Lista de campañas WhatsApp (filtra <c>Canal == 2</c>).</summary>
        Task<CampaignResult<List<Campana>>> ListWhatsAppAsync(CancellationToken ct = default);

        /// <summary>Detalle de una campaña.</summary>
        Task<CampaignResult<Campana>> GetAsync(int id, CancellationToken ct = default);

        /// <summary>Prepara los destinatarios de una campaña.</summary>
        Task<CampaignResult> PrepareRecipientsAsync(int campaignId, CancellationToken ct = default);

        /// <summary>Destinatarios de una campaña.</summary>
        Task<CampaignResult<List<CampanaWhatsAppDestinatario>>> GetRecipientsAsync(int campaignId, CancellationToken ct = default);

        /// <summary>Marca un destinatario como enviado.</summary>
        Task<CampaignResult> MarkSentAsync(int recipientId, CancellationToken ct = default);

        /// <summary>Marca un destinatario como pendiente (deshacer envío).</summary>
        Task<CampaignResult> MarkPendingAsync(int recipientId, CancellationToken ct = default);
    }
}
