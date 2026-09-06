using ExpenseTracker.Application.Features.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
namespace ExpenseTracker.Api.Controllers;

[ApiController, Route("api/auth"), EnableRateLimiting("auth")]
public sealed class AuthController(AuthService auth) : ControllerBase
{
    [HttpPost("register"), AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        var result = await auth.Register(request, ct);
        return Created("/api/auth/me", result);
    }
    [HttpPost("login"), AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct) => Ok(await auth.Login(request, ct));
    [HttpGet("me"), Authorize]
    public async Task<ActionResult<UserResponse>> Me(CancellationToken ct) => Ok(await auth.Me(ct));
}
