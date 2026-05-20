using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Rag.Application.DTOs;
using Rag.Application.Exceptions;
using Rag.Application.Interfaces;
using Rag.Domain.Entities;
using Rag.Infrastructure.Persistence;
using Rag.Shared.Options;

namespace Rag.Infrastructure.Services;

public class AuthService(ApplicationDbContext dbContext, IOptions<JwtSettings> jwtOptions, ILogger<AuthService> logger) : IAuthService
{
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var exists = await dbContext.Users.AnyAsync(x => x.Email == request.Email, ct);
        if (exists) throw new ValidationException("Email is already registered");

        var userRole = await dbContext.Roles.FirstAsync(x => x.Name == "User", ct);
        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = true
        };

        dbContext.Users.Add(user);
        dbContext.UserRoles.Add(new UserRole { User = user, RoleId = userRole.Id });
        await dbContext.SaveChangesAsync(ct);

        return await BuildAuthResponseAsync(user, ct);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await dbContext.Users
            .Include(x => x.UserRoles).ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Email == request.Email, ct)
            ?? throw new UnauthorizedException("Invalid credentials");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid credentials");

        if (!user.IsActive)
            throw new ForbiddenException("User account is inactive");

        return await BuildAuthResponseAsync(user, ct);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var token = await dbContext.RefreshTokens
            .Include(x => x.User).ThenInclude(x => x.UserRoles).ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Token == refreshToken, ct)
            ?? throw new UnauthorizedException("Invalid refresh token");

        if (token.IsRevoked || token.ExpiresAt <= DateTime.UtcNow)
            throw new UnauthorizedException("Refresh token is expired or revoked");

        token.IsRevoked = true;
        token.RevokedAt = DateTime.UtcNow;

        return await BuildAuthResponseAsync(token.User, ct);
    }

    public async Task RevokeTokenAsync(Guid userId, string refreshToken, CancellationToken ct = default)
    {
        var token = await dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken && x.UserId == userId, ct)
            ?? throw new NotFoundException("Refresh token not found");

        token.IsRevoked = true;
        token.RevokedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(ct);
    }

    private async Task<AuthResponse> BuildAuthResponseAsync(User user, CancellationToken ct)
    {
        var token = GenerateJwtToken(user);
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            IsRevoked = false
        });

        await dbContext.SaveChangesAsync(ct);
        logger.LogInformation("Authentication successful for user {UserId}", user.Id);

        var roles = user.UserRoles.Select(x => x.Role.Name).ToList();
        return new AuthResponse(token, refreshToken, new UserDto(user.Id, user.Email, user.FirstName, user.LastName, roles, user.IsActive));
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email)
        };
        claims.AddRange(user.UserRoles.Select(ur => new Claim(ClaimTypes.Role, ur.Role.Name)));

        var token = new JwtSecurityToken(
            _jwtSettings.Issuer,
            _jwtSettings.Audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
