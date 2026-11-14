using InsigniaConsultoresApp.Models.Requests;
using InsigniaConsultoresApp.Models.Responses;

namespace InsigniaConsultoresApp.Services.Api
{
    /// <summary>
    /// Contrato del servicio de autenticación OAuth2 (password grant).
    /// </summary>
    public interface IAuthService
    {
        Task<LoginResult> LoginAsync(TokenRequest request, CancellationToken cancellationToken = default);
    }
}
