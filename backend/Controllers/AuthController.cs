using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var response = await _authService.AuthenticateAsync(dto);
        if (response is null)
            return Unauthorized(new { message = "Email ou senha inválidos." });

        var token = _authService.GenerateToken(
            dto.Email,
            response.Name,
            DateTime.UtcNow.AddHours(8)
        );

        Response.Cookies.Append(
            "auth_token",
            token,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = response.ExpiresAt,
                Path = "/",
            }
        );

        return Ok(response);
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Append(
            "auth_token",
            "",
            new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(-1),
                Path = "/",
            }
        );

        return Ok(new { message = "Logout realizado." });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "unknown";
        var name = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "unknown";
        return Ok(
            new LoginResponseDto
            {
                Email = email,
                Name = name,
                ExpiresAt = DateTime.UtcNow,
            }
        );
    }
}
