using InsigniaConsultoresApp.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;

namespace InsigniaConsultoresApp.Services.Storage
{
    /// <summary>
    /// Persistencia híbrida de sesión: token cifrado en SecureStorage + metadata en SQLite.
    /// Degrada defensivamente a SQLite sin cifrar si el keystore del dispositivo no está disponible.
    /// </summary>
    public sealed class TokenStore : ITokenStore
    {
        private const string AccessTokenKey = "access_token";

        private readonly SessionDatabase _db;
        private readonly ILogger<TokenStore>? _logger;

        public TokenStore(SessionDatabase db, ILogger<TokenStore>? logger = null)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<bool> HasValidSessionAsync()
        {
            var session = await GetSessionAsync();
            return session is not null && session.ExpiresAtUtc > DateTime.UtcNow;
        }

        public async Task<SessionModel?> GetSessionAsync()
        {
            var session = await _db.GetAsync();
            if (session is null)
                return null;

            if (session.IsEncrypted)
            {
                try
                {
                    // El token vive cifrado en SecureStorage; recuperarlo desde allí.
                    session.AccessToken = await SecureStorage.Default.GetAsync(AccessTokenKey);
                }
                catch (Exception ex)
                {
                    // Keystore no disponible al leer: la sesión queda sin token (seguro).
                    _logger?.LogWarning(ex, "No se pudo leer el token de SecureStorage");
                    session.AccessToken = null;
                }
            }

            return session;
        }

        public async Task SaveSessionAsync(SessionModel session)
        {
            bool encrypted = false;
            try
            {
                await SecureStorage.Default.SetAsync(AccessTokenKey, session.AccessToken ?? "");
                encrypted = true;
            }
            catch (Exception ex)
            {
                // Keystore del dispositivo no disponible: degradar a SQLite sin cifrar.
                _logger?.LogWarning(ex, "SecureStorage no disponible; token en SQLite sin cifrar");
                encrypted = false;
            }

            session.IsEncrypted = encrypted;
            await _db.SaveAsync(session);
        }

        public async Task ClearAsync()
        {
            // Limpiar ambos: SecureStorage y la fila de SQLite.
            try
            {
                SecureStorage.Default.Remove(AccessTokenKey);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "No se pudo limpiar SecureStorage");
            }

            await _db.ClearAsync();
        }
    }
}
