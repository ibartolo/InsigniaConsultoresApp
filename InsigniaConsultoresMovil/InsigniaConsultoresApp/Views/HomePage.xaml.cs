using InsigniaConsultoresApp.ViewModels;
using Microsoft.Maui.Controls;

namespace InsigniaConsultoresApp.Views
{
    /// <summary>
    /// Pantalla Home post-autenticación (placeholder en blanco). BindingContext inyectado por constructor.
    /// </summary>
    public partial class HomePage : ContentPage
    {
        private readonly HomeViewModel _viewModel;

        public HomePage(HomeViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Carga el nombre de usuario desde la sesión persistida (para el flyout).
            await _viewModel.LoadAsync();
        }
    }
}
