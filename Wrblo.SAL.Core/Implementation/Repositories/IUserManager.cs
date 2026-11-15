using Wrblo.SAL.Shared.DTO.Request;
using Wrblo.SAL.Shared.DTO.Response;
using Wrblo.SAL.Shared.ViewModels;

namespace Wrblo.SAL.Core.Implementation.Repositories;

public interface IUserManager
{
    Task<ApiResponse<LoginResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<LoginResponse>> LoginWithGoogleAsync(GoogleLoginRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<LoginResponse>> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserResponse>> GetCurrentUserAsync(string userId, CancellationToken cancellationToken = default);
}