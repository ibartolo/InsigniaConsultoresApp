using System.Globalization;
using Microsoft.Maui.Controls;

namespace InsigniaConsultoresApp.Converters
{
    /// <summary>
    /// Invierte un valor booleano: <c>true → false</c> y <c>false → true</c>.
    /// Se usa para alternar la visibilidad de los botones Enviar/Deshacer según <c>IsSent</c>.
    /// </summary>
    public sealed class InvertedBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is bool b && !b;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is bool b && !b;
    }
}
