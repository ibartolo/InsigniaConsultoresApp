namespace InsigniaConsultoresApp.Validations
{
    /// <summary>
    /// Resultado de validación (lista de mensajes).
    /// </summary>
    public sealed class ValidationResult
    {
        public bool IsValid => Messages.Count == 0;
        public IReadOnlyList<string> Messages { get; }

        public ValidationResult(IReadOnlyList<string> messages) => Messages = messages;
    }

    /// <summary>
    /// Validación síncrona pura de los campos del login.
    /// </summary>
    public static class LoginValidator
    {
        public static ValidationResult Validate(string? username, string? password)
        {
            var messages = new List<string>();
            if (string.IsNullOrWhiteSpace(username))
                messages.Add("Ingresa tu usuario");
            if (string.IsNullOrWhiteSpace(password))
                messages.Add("Ingresa tu contraseña");
            return new ValidationResult(messages);
        }
    }
}
