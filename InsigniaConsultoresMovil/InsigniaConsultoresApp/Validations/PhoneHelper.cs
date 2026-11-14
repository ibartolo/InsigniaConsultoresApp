using System.Linq;

namespace InsigniaConsultoresApp.Validations
{
    /// <summary>
    /// Helper puro para normalizar teléfonos mexicanos a 10 dígitos y construir
    /// el deep link de WhatsApp (prefijo 52). Sin estado y sin dependencias.
    /// </summary>
    public static class PhoneHelper
    {
        /// <summary>
        /// Conserva solo los dígitos del teléfono; si quedan más de 10 se toman los
        /// ÚLTIMOS 10 (para descartar el <c>+52</c>/<c>044</c>/cero inicial); si quedan
        /// menos de 10 no es enviable.
        /// </summary>
        public static bool TryNormalizeTo10Digits(string? raw, out string ten)
        {
            ten = string.Empty;

            if (string.IsNullOrWhiteSpace(raw))
                return false;

            var digits = new string(raw.Where(char.IsDigit).ToArray());
            if (digits.Length < 10)
                return false;

            ten = digits.Length > 10 ? digits[^10..] : digits;
            return true;
        }

        /// <summary>
        /// Construye el URI <c>https://wa.me/52{tenDigits}?text={message escapado}</c>.
        /// Se asume que <paramref name="tenDigits"/> ya fue validado a 10 dígitos.
        /// </summary>
        public static string BuildWhatsAppUri(string tenDigits, string? message)
        {
            var text = Uri.EscapeDataString(message ?? string.Empty);
            return $"https://wa.me/52{tenDigits}?text={text}";
        }
    }
}
