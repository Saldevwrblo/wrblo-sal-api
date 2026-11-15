using Wrblo.SAL.Shared.Entities;
using Wrblo.SAL.Shared.DTO.Response;

namespace Wrblo.SAL.Core.Implementation.Repositories;

public interface IJwtTokenService
{
    TokenPair CreateTokens(ApplicationUser user);
    string HashRefreshToken(string refreshToken);
}

public sealed record TokenPair(LoginResponse Response, string RefreshTokenHash);