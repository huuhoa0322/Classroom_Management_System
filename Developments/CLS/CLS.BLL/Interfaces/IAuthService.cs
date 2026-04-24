using CLS.BLL.Common;
using CLS.BLL.DTOs.Auth;

namespace CLS.BLL.Interfaces;

public interface IAuthService
{
    Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
}
