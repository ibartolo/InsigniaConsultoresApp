using InsigniaConsultoresApp.ViewModels;
using Microsoft.Maui.Controls;

namespace InsigniaConsultoresApp.Views
{
    /// <summary>
    /// Destinatarios de una campaña. Recibe el id por query string y carga la lista al aparecer.
    /// </summary>
    [QueryProperty(nameof(CampaignId), "campaignId")]
    public partial class RecipientsPage : ContentPage
    {
        private readonly RecipientsViewModel _viewModel;

        // Solo se carga una vez por navegación: al volver de WhatsApp no se recarga la lista,
        // así no se pierde el estado optimista (Enviado) ni se muestra información vieja.
        private bool _loaded;

        public RecipientsPage(RecipientsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _viewModel = viewModel;
        }

        /// <summary>
        /// Id de campaña recibido por navegación (clave "campaignId").
        /// Se parsea a entero y se asigna al ViewModel antes de <see cref="OnAppearing"/>.
        /// </summary>
        public string? CampaignId
        {
            set
            {
                if (int.TryParse(value, out var id))
                    _viewModel.CampaignId = id;
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_loaded)
                return;

            _loaded = true;
            await _viewModel.LoadAsync();
        }
    }
}
