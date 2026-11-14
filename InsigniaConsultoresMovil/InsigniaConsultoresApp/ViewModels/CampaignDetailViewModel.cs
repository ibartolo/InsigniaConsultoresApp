using System.Threading.Tasks;
using System.Windows.Input;
using InsigniaConsultoresApp.Models.Responses;
using InsigniaConsultoresApp.Services.Api;
using InsigniaConsultoresApp.Services.Storage;
using Microsoft.Maui.Controls;

namespace InsigniaConsultoresApp.ViewModels
{
    /// <summary>
    /// Detalle de una campaña: carga datos frescos por id y expone el comando "Comenzar campaña".
    /// </summary>
    public sealed class CampaignDetailViewModel : CampaignViewModelBase
    {
        private readonly ICampaignService _campaignService;

        private Campana? _campaign;

        public CampaignDetailViewModel(ICampaignService campaignService, ITokenStore tokenStore)
            : base(tokenStore)
        {
            _campaignService = campaignService;

            // Habilitado solo cuando ya hay una campaña cargada.
            StartCampaignCommand = new Command(async () => await StartCampaignAsync(), () => Campaign is not null);
        }

        public int CampaignId { get; set; }

        public Campana? Campaign
        {
            get => _campaign;
            private set
            {
                if (SetProperty(ref _campaign, value))
                    ((Command)StartCampaignCommand).ChangeCanExecute();
            }
        }

        public ICommand StartCampaignCommand { get; }

        public async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                var result = await _campaignService.GetAsync(CampaignId);
                await HandleUnauthorizedAsync(result.Kind);

                if (!result.IsSuccess || result.Value is null)
                {
                    ErrorMessage = result.Message;
                    return;
                }

                Campaign = result.Value;
            });
        }

        private async Task StartCampaignAsync()
        {
            if (Campaign is null)
                return;

            await Shell.Current.GoToAsync($"campaign-recipients?campaignId={CampaignId}");
        }
    }
}
