using CLS.BLL.DTOs.Auth;

namespace CLS.BLL.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
}
