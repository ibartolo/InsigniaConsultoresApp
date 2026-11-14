using InsigniaConsultoresApp.ViewModels;
using Microsoft.Maui.Controls;

namespace InsigniaConsultoresApp.Views
{
    /// <summary>
    /// Detalle de una campaña. Recibe el id por query string y carga los datos frescos al aparecer.
    /// </summary>
    [QueryProperty(nameof(CampaignId), "campaignId")]
    public partial class CampaignDetailPage : ContentPage
    {
        private readonly CampaignDetailViewModel _viewModel;

        public CampaignDetailPage(CampaignDetailViewModel viewModel)
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
            await _viewModel.LoadAsync();
        }
    }
}
