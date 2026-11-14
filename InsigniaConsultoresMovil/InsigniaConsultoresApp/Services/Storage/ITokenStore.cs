using InsigniaConsultoresApp.Models;

namespace InsigniaConsultoresApp.Services.Storage
{
    /// <summary>
    /// Contrato de persistencia de sesión (SecureStorage + SQLite).
    /// </summary>
    public interface ITokenStore
    {
        Task<bool> HasValidSessionAsync();
        Task<SessionModel?> GetSessionAsync();
        Task SaveSessionAsync(SessionModel session);
        Task ClearAsync();
    }
}
