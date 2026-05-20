using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rag.API.Extensions;
using Rag.Application.DTOs;
using Rag.Application.Interfaces;

namespace Rag.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct)
        => Ok(await authService.RegisterAsync(request, ct));

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
        => Ok(await authService.LoginAsync(request, ct));

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshTokenRequest request, CancellationToken ct)
        => Ok(await authService.RefreshTokenAsync(request.RefreshToken, ct));

    [Authorize]
    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke(RevokeTokenRequest request, CancellationToken ct)
    {
        await authService.RevokeTokenAsync(User.GetUserId(), request.RefreshToken, ct);
        return NoContent();
    }
}
