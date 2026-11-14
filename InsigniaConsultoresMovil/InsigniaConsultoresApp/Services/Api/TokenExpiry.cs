namespace InsigniaConsultoresApp.Services.Api
{
    /// <summary>
    /// Cálculo puro de la expiración del token con margen de seguridad.
    /// </summary>
    public static class TokenExpiry
    {
        // expires_at = now + expires_in - margen; si expires_in falta o <= margen,
        // se usa expires_in sin margen (nunca negativo). null => caduca ya (seguro).
        public static DateTime CalculateUtc(DateTime nowUtc, int? expiresInSeconds)
        {
            int lifetime = expiresInSeconds ?? 0;
            int effective = lifetime > AuthConfig.TokenExpiryMarginSeconds
                ? lifetime - AuthConfig.TokenExpiryMarginSeconds
                : lifetime;
            return nowUtc.AddSeconds(effective); // siempre >= nowUtc
        }
    }
}
