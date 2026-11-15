using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Wrblo.SAL.Core.Implementation.Repositories;
using Wrblo.SAL.Shared.DTO.Request;
using Wrblo.SAL.Shared.DTO.Response;
using Wrblo.SAL.Shared.Entities;
using Wrblo.SAL.Shared.ViewModels;

namespace Wrblo.SAL.Core.Implementation.Services;

public sealed class UserManager(
    IUserRepository userRepository,
    IPasswordHasher<ApplicationUser> passwordHasher,
    IJwtTokenService tokenService,
    IOptions<GoogleOptions> googleOptions) : IUserManager
{
    private readonly GoogleOptions _googleOptions = googleOptions.Value;

    public async Task<ApiResponse<LoginResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);
        if (await userRepository.GetByEmailAsync(email, cancellationToken) is not null)
        {
            return ApiResponse<LoginResponse>.Conflict("An account with this email already exists.");
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Password = string.Empty,
            Role = Roles.User,
            CreatedOn = DateTime.UtcNow,
            IsEmailVerified = false
        };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        var response = await CreateTokenResponseAsync(user, cancellationToken, addUser: true);
        return response;
    }

    public async Task<ApiResponse<LoginResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByEmailAsync(request.Username, cancellationToken);
        if (user is null || string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return ApiResponse<LoginResponse>.Unauthorized("Invalid email or password.");
        }

        var verification = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            return ApiResponse<LoginResponse>.Unauthorized("Invalid email or password.");
        }

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        }

        return await CreateTokenResponseAsync(user, cancellationToken);
    }

    public async Task<ApiResponse<LoginResponse>> LoginWithGoogleAsync(
        GoogleLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var audienceIds = _googleOptions.GetAudienceIds();
        if (audienceIds.Count == 0)
        {
            return ApiResponse<LoginResponse>.Fail("Google authentication is not configured.");
        }

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(
                request.IdToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = audienceIds
                });
        }
        catch (InvalidJwtException)
        {
            return ApiResponse<LoginResponse>.Unauthorized("The Google ID token is invalid or expired.");
        }

        if (!payload.EmailVerified || string.IsNullOrWhiteSpace(payload.Email))
        {
            return ApiResponse<LoginResponse>.Unauthorized("The Google account email is not verified.");
        }

        var user = await userRepository.GetByGoogleIdAsync(payload.Subject, cancellationToken)
            ?? await userRepository.GetByEmailAsync(payload.Email, cancellationToken);

        if (user is null)
        {
            user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = NormalizeEmail(payload.Email),
                NormalizedUserName = NormalizeEmail(payload.Email).ToUpperInvariant(),
                Email = NormalizeEmail(payload.Email),
                NormalizedEmail = NormalizeEmail(payload.Email).ToUpperInvariant(),
                FirstName = payload.GivenName ?? string.Empty,
                LastName = payload.FamilyName ?? string.Empty,
                Password = string.Empty,
                GoogleId = payload.Subject,
                Role = Roles.User,
                IsEmailVerified = true,
                CreatedOn = DateTime.UtcNow
            };

            return await CreateTokenResponseAsync(user, cancellationToken, addUser: true);
        }

        user.GoogleId = payload.Subject;
        user.IsEmailVerified = true;
        if (string.IsNullOrWhiteSpace(user.FirstName))
        {
            user.FirstName = payload.GivenName ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(user.LastName))
        {
            user.LastName = payload.FamilyName ?? string.Empty;
        }

        return await CreateTokenResponseAsync(user, cancellationToken);
    }

    public async Task<ApiResponse<LoginResponse>> RefreshAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        var refreshTokenHash = tokenService.HashRefreshToken(request.RefreshToken);
        var user = await userRepository.GetByRefreshTokenHashAsync(refreshTokenHash, cancellationToken);
        if (user is null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return ApiResponse<LoginResponse>.Unauthorized("The refresh token is invalid or expired.");
        }

        return await CreateTokenResponseAsync(user, cancellationToken);
    }

    public async Task<ApiResponse<UserResponse>> GetCurrentUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        return user is null
            ? ApiResponse<UserResponse>.NotFound("User not found.")
            : ApiResponse<UserResponse>.Success(ToUserResponse(user));
    }

    private async Task<ApiResponse<LoginResponse>> CreateTokenResponseAsync(
        ApplicationUser user,
        CancellationToken cancellationToken,
        bool addUser = false)
    {
        var tokenPair = tokenService.CreateTokens(user);
        user.RefreshToken = tokenPair.RefreshTokenHash;
        user.RefreshTokenExpiryTime = tokenPair.Response.RefreshTokenExpiresAt;
        user.ModifiedOn = DateTime.UtcNow;

        if (addUser)
        {
            await userRepository.AddAsync(user, cancellationToken);
        }
        else
        {
            await userRepository.UpdateAsync(user, cancellationToken);
        }

        return ApiResponse<LoginResponse>.Success(tokenPair.Response);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static UserResponse ToUserResponse(ApplicationUser user) => new()
    {
        Id = user.Id,
        Email = user.Email ?? string.Empty,
        UserName = user.UserName ?? string.Empty,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Role = user.Role.ToString()
    };
}