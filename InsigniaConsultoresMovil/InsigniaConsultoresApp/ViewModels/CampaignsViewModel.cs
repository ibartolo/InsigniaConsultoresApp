using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using InsigniaConsultoresApp.Models.Responses;
using InsigniaConsultoresApp.Services.Api;
using InsigniaConsultoresApp.Services.Storage;
using Microsoft.Maui.Controls;

namespace InsigniaConsultoresApp.ViewModels
{
    /// <summary>
    /// Listado de campañas WhatsApp (Canal == 2): carga, refresco y apertura del detalle.
    /// </summary>
    public sealed class CampaignsViewModel : CampaignViewModelBase
    {
        private readonly ICampaignService _campaignService;

        private bool _isRefreshing;

        public CampaignsViewModel(ICampaignService campaignService, ITokenStore tokenStore)
            : base(tokenStore)
        {
            _campaignService = campaignService;

            Campaigns = new ObservableCollection<Campana>();

            LoadCommand = new Command(async () => await LoadAsync());
            RefreshCommand = new Command(async () => await RefreshAsync());
            OpenCommand = new Command<Campana>(async campaign => await OpenCampaignAsync(campaign));
        }

        public ObservableCollection<Campana> Campaigns { get; }

        /// <summary>True mientras se ejecuta el pull-to-refresh (RefreshView).</summary>
        public bool IsRefreshing
        {
            get => _isRefreshing;
            private set => SetProperty(ref _isRefreshing, value);
        }

        public ICommand LoadCommand { get; }

        public ICommand RefreshCommand { get; }

        /// <summary>Abre el detalle de la campaña seleccionada.</summary>
        public ICommand OpenCommand { get; }

        public async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                var result = await _campaignService.ListWhatsAppAsync();
                await HandleUnauthorizedAsync(result.Kind);

                if (!result.IsSuccess || result.Value is null)
                {
                    ErrorMessage = result.Message;
                    return;
                }

                Campaigns.Clear();
                foreach (var campaign in result.Value)
                    Campaigns.Add(campaign);

                IsEmpty = Campaigns.Count == 0;
            });
        }

        public async Task OpenCampaignAsync(Campana campaign)
        {
            await Shell.Current.GoToAsync($"campaign-detail?campaignId={campaign.Id}");
        }

        private async Task RefreshAsync()
        {
            IsRefreshing = true;
            try
            {
                await LoadAsync();
            }
            finally
            {
                IsRefreshing = false;
            }
        }
    }
}
