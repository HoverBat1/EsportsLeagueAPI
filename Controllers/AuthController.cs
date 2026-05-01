using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EsportsLeagueApi01.Data;
using EsportsLeagueApi01.Models;
using EsportsLeagueApi01.DTOs;
using EsportsLeagueApi01.Services;
using Microsoft.AspNetCore.Authorization;

namespace EsportsLeagueApi01.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly LeagueDbContext _db;
    private readonly TokenService _tokenService;
    private readonly IConfiguration _config;

    public AuthController(LeagueDbContext db, TokenService tokenService, IConfiguration config)
    {
        _db = db;
        _tokenService = tokenService;
        _config = config;
    }




    // POST api/auth/register
    [Authorize]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Username == request.Username))
        {
            return BadRequest($"Username \"{request.Username}\" already taken");
        }

        var user = new User
        {
            Username = request.Username, 
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = _tokenService.GenerateToken(user);
        return Ok(new AuthResponse
        {
            Token = token, 
            Username = user.Username, 
            ExpiresAt = DateTime.UtcNow.AddHours(double.Parse(_config["Jwt:ExpiryHours"]!))
        });
    }

    // POST api/auth/login
    [Authorize]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized("Invalid username or password");
        }

        var token = _tokenService.GenerateToken(user);
        return Ok(new AuthResponse
        {
            Token = token, 
            Username = user.Username, 
            ExpiresAt = DateTime.UtcNow.AddHours(double.Parse(_config["Jwt:ExpiryHours"]!))
        });
    }
}