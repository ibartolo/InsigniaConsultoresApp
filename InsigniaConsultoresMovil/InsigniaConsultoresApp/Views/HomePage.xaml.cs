using InsigniaConsultoresApp.ViewModels;
using Microsoft.Maui.Controls;

namespace InsigniaConsultoresApp.Views
{
    /// <summary>
    /// Pantalla Home post-autenticación: sección de indicadores (KPIs) con selector de
    /// rango de fechas. BindingContext inyectado por constructor; la carga inicial corre una sola vez.
    /// </summary>
    public partial class HomePage : ContentPage
    {
        private readonly HomeViewModel _viewModel;
        private bool _loaded;

        public HomePage(HomeViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Carga una sola vez (nombre de usuario + indicadores); al volver de otra
            // pantalla no se recarga.
            if (_loaded)
                return;
            _loaded = true;

            await _viewModel.LoadAsync();
        }
    }
}
