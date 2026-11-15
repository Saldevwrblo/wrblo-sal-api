using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wrblo.SAL.Core.Implementation.Repositories;
using Wrblo.SAL.Shared.DTO.Request;
using Wrblo.SAL.Shared.DTO.Response;
using Wrblo.SAL.Shared.ViewModels;

namespace Wrblo_SAL.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(IUserManager userManager) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var response = await userManager.RegisterAsync(request, cancellationToken);
        return ToActionResult(response);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await userManager.LoginAsync(request, cancellationToken);
        return ToActionResult(response);
    }

    [AllowAnonymous]
    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin(
        GoogleLoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await userManager.LoginWithGoogleAsync(request, cancellationToken);
        return ToActionResult(response);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var response = await userManager.RefreshAsync(request, cancellationToken);
        return ToActionResult(response);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return StatusCode(
                ApiResponseCode.Unauthorized,
                ApiResponse<UserResponse>.Unauthorized("The access token does not contain a user identifier."));
        }

        var response = await userManager.GetCurrentUserAsync(userId, cancellationToken);
        return ToActionResult(response);
    }

    private static IActionResult ToActionResult<T>(ApiResponse<T> response) =>
        new ObjectResult(response) { StatusCode = response.Code };
}
