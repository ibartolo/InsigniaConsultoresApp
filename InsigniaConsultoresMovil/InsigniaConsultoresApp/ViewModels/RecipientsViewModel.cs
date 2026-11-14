using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using InsigniaConsultoresApp.Models.Responses;
using InsigniaConsultoresApp.Services.Api;
using InsigniaConsultoresApp.Services.Storage;
using InsigniaConsultoresApp.Validations;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace InsigniaConsultoresApp.ViewModels
{
    /// <summary>
    /// Pantalla de destinatarios: prepara (si hace falta), carga y filtra destinatarios,
    /// y orquesta el envío/deshacer por WhatsApp con actualización optimista y rollback.
    /// </summary>
    public sealed class RecipientsViewModel : CampaignViewModelBase
    {
        private readonly ICampaignService _campaignService;

        /// <summary>Lista completa de destinatarios (fuente del filtro local).</summary>
        private readonly List<RecipientItemViewModel> _all = new();

        private string _aspectoMensaje = string.Empty;
        private string? _searchText;
        private string? _emptyMessage;

        public RecipientsViewModel(ICampaignService campaignService, ITokenStore tokenStore)
            : base(tokenStore)
        {
            _campaignService = campaignService;

            Recipients = new ObservableCollection<RecipientItemViewModel>();
            LoadCommand = new Command(async () => await LoadAsync());
        }

        public int CampaignId { get; set; }

        /// <summary>Asunto de la campaña, usado como texto del mensaje de WhatsApp.</summary>
        public string AspectoMensaje
        {
            get => _aspectoMensaje;
            private set => SetProperty(ref _aspectoMensaje, value);
        }

        /// <summary>Texto de búsqueda; al cambiarlo se aplica el filtro local (CW-R4).</summary>
        public string? SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    ApplyFilter();
            }
        }

        /// <summary>Lista filtrada enlazada a la CollectionView.</summary>
        public ObservableCollection<RecipientItemViewModel> Recipients { get; }

        /// <summary>Mensaje cuando la lista está vacía (sin datos o sin resultados de búsqueda).</summary>
        public string? EmptyMessage
        {
            get => _emptyMessage;
            private set => SetProperty(ref _emptyMessage, value);
        }

        public ICommand LoadCommand { get; }

        public async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                var campaign = await _campaignService.GetAsync(CampaignId);
                await HandleUnauthorizedAsync(campaign.Kind);

                if (!campaign.IsSuccess || campaign.Value is null)
                {
                    ErrorMessage = campaign.Message;
                    return;
                }

                // El mensaje que se envía por WhatsApp es la DESCRIPCIÓN de la campaña
                // (el Asunto es solo el nombre corto, se usa como respaldo).
                AspectoMensaje = campaign.Value.Descripcion
                    ?? campaign.Value.Asunto
                    ?? string.Empty;

                // Preparación idempotente: solo si el servidor indica que aún no está preparada (CW-R3).
                if (!campaign.Value.Preparada)
                {
                    var prepare = await _campaignService.PrepareRecipientsAsync(CampaignId);
                    await HandleUnauthorizedAsync(prepare.Kind);

                    if (!prepare.IsSuccess)
                    {
                        ErrorMessage = prepare.Message;
                        return;
                    }
                }

                var recipients = await _campaignService.GetRecipientsAsync(CampaignId);
                await HandleUnauthorizedAsync(recipients.Kind);

                if (!recipients.IsSuccess || recipients.Value is null)
                {
                    ErrorMessage = recipients.Message;
                    return;
                }

                _all.Clear();
                foreach (var dto in recipients.Value)
                    _all.Add(new RecipientItemViewModel(dto, SendAsync, UndoAsync));

                ApplyFilter();
            });
        }

        /// <summary>
        /// Envía por WhatsApp (CW-R6): normaliza, abre el deep link y, tras lanzarlo, marca
        /// Enviado de forma optimista; si el marcado falla, revierte a Pendiente.
        /// </summary>
        public async Task SendAsync(RecipientItemViewModel item)
        {
            var ten = item.TenDigits;
            if (ten is null)
                return;

            var uri = PhoneHelper.BuildWhatsAppUri(ten, AspectoMensaje);

            // Lanzar el deep link dentro de try/catch: si falla (WhatsApp no instalado),
            // avisa y NO marca como enviado. Se usa OpenAsync en lugar de TryOpenAsync
            // porque en Android 11+ la visibilidad de paquetes hace que TryOpenAsync
            // devuelva false aunque el intent sí se pueda resolver.
            try
            {
                await Launcher.Default.OpenAsync(new Uri(uri));
            }
            catch (Exception)
            {
                ErrorMessage = "No se pudo abrir WhatsApp. Verifica que esté instalado.";
                return;
            }

            // Actualización optimista tras lanzar WhatsApp.
            item.ApplySent(DateTime.UtcNow);

            var mark = await _campaignService.MarkSentAsync(item.Id);
            await HandleUnauthorizedAsync(mark.Kind);

            if (!mark.IsSuccess)
            {
                item.ApplyPending();
                ErrorMessage = mark.Message;
            }
        }

        /// <summary>
        /// Deshace un envío (CW-R7): marca Pendiente contra el backend; en éxito actualiza
        /// la fila, en fallo muestra el error.
        /// </summary>
        public async Task UndoAsync(RecipientItemViewModel item)
        {
            var mark = await _campaignService.MarkPendingAsync(item.Id);
            await HandleUnauthorizedAsync(mark.Kind);

            if (mark.IsSuccess)
                item.ApplyPending();
            else
                ErrorMessage = mark.Message;
        }

        /// <summary>
        /// Reconstruye <see cref="Recipients"/> a partir de <see cref="_all"/> según <see cref="SearchText"/>
        /// (coincidencia "contiene" sin distinguir mayúsculas sobre Nombre o Telefono), y actualiza
        /// los estados de vacío con su mensaje correspondiente.
        /// </summary>
        private void ApplyFilter()
        {
            Recipients.Clear();

            var text = SearchText?.Trim();
            IEnumerable<RecipientItemViewModel> filtered = _all;

            if (!string.IsNullOrEmpty(text))
            {
                filtered = _all.Where(i =>
                    i.Nombre.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                    i.Telefono.Contains(text, StringComparison.OrdinalIgnoreCase));
            }

            foreach (var item in filtered)
                Recipients.Add(item);

            IsEmpty = Recipients.Count == 0;
            EmptyMessage = IsEmpty
                ? (string.IsNullOrEmpty(text)
                    ? "No hay destinatarios para esta campaña"
                    : $"Sin resultados para «{text}»")
                : null;
        }
    }
}
