using System.Linq;
using InsigniaConsultoresApp.Models.Responses;
using InsigniaConsultoresApp.ViewModels;
using Microsoft.Maui.Controls;

namespace InsigniaConsultoresApp.Views
{
    /// <summary>
    /// Lista de campañas WhatsApp. BindingContext inyectado por constructor; la carga inicial corre una sola vez.
    /// </summary>
    public partial class CampaignsPage : ContentPage
    {
        private readonly CampaignsViewModel _viewModel;
        private bool _loaded;

        public CampaignsPage(CampaignsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_loaded)
                return;
            _loaded = true;

            await _viewModel.LoadAsync();
        }

        /// <summary>
        /// Abre el detalle de la campaña tocada y limpia la selección
        /// (permite volver a tocar la misma fila al regresar a la lista).
        /// </summary>
        private void OnCampaignSelected(object? sender, SelectionChangedEventArgs e)
        {
            var collection = (CollectionView)sender!;

            if (e.CurrentSelection.FirstOrDefault() is Campana campaign && _viewModel.OpenCommand.CanExecute(campaign))
                _viewModel.OpenCommand.Execute(campaign);

            collection.SelectedItem = null;
        }
    }
}
