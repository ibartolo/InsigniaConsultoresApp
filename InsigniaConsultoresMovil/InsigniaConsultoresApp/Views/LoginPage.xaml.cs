using InsigniaConsultoresApp.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace InsigniaConsultoresApp.Views
{
    /// <summary>
    /// Pantalla de login (sin flyout, sin barra de navegación). BindingContext inyectado por constructor.
    /// </summary>
    public partial class LoginPage : ContentPage
    {
        // Colores de la línea inferior del campo (espejo de Colors.xaml: BrandOrange / BrandInputBorder).
        private static readonly Color FocusLineColor = Color.FromArgb("#E8952B");
        private static readonly Color NormalLineColor = Color.FromArgb("#dde5ee");
        private const double FocusLineHeight = 2;
        private const double NormalLineHeight = 1;

        public LoginPage(LoginViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        private void OnUsernameFocused(object? sender, FocusEventArgs e) => SetLine(UsernameLine, focused: true);
        private void OnUsernameUnfocused(object? sender, FocusEventArgs e) => SetLine(UsernameLine, focused: false);
        private void OnPasswordFocused(object? sender, FocusEventArgs e) => SetLine(PasswordLine, focused: true);
        private void OnPasswordUnfocused(object? sender, FocusEventArgs e) => SetLine(PasswordLine, focused: false);

        // Línea inferior del input: 1px #dde5ee en reposo → 2px #E8952B en foco.
        private void SetLine(BoxView line, bool focused)
        {
            if (focused)
            {
                line.Color = FocusLineColor;
                line.HeightRequest = FocusLineHeight;
            }
            else
            {
                line.Color = NormalLineColor;
                line.HeightRequest = NormalLineHeight;
            }
        }
    }
}
