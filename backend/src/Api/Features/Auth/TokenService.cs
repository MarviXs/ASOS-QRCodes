using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Fei.Is.Api.Features.Auth;

public class TokenService
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    private readonly IConfiguration _configuration;

    public TokenService(AppDbContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        _context = context;
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task<string> CreateAccessToken(ApplicationUser user)
    {
        var signingCredentials = GetSigningCredentials();
        var claims = await GetClaims(user);
        var tokenOptions = GenerateTokenOptions(signingCredentials, claims);
        return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
    }

    public async Task<string> CreateRefreshToken(ApplicationUser user)
    {
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = Guid.NewGuid(),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(365).ToUnixTimeSeconds(),
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();

        // Convert the token to a Base64 encoded string
        var tokenBytes = Encoding.UTF8.GetBytes(refreshToken.Token.ToString());
        var base64Token = Convert.ToBase64String(tokenBytes);

        return base64Token;
    }

    public Guid RefreshTokenToGuid(string refreshToken)
    {
        byte[] tokenBytes;
        try
        {
            tokenBytes = Convert.FromBase64String(refreshToken);
        }
        catch (FormatException)
        {
            return Guid.Empty;
        }

        refreshToken = Encoding.UTF8.GetString(tokenBytes);

        if (!Guid.TryParse(refreshToken, out var token))
        {
            return Guid.Empty;
        }

        return token;
    }

    private SigningCredentials GetSigningCredentials()
    {
        var secret = _configuration["JwtSettings:secret"];
        if (secret == null)
        {
            throw new Exception("JWT secret is not set");
        }

        var key = Encoding.UTF8.GetBytes(secret);

        var secretKey = new SymmetricSecurityKey(key);
        return new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
    }

    private async Task<List<Claim>> GetClaims(ApplicationUser user)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, user.Id.ToString()), new(ClaimTypes.Email, user.Email!) };
        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        return claims;
    }

    private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var tokenOptions = new JwtSecurityToken(
            issuer: jwtSettings["validIssuer"],
            audience: jwtSettings["validAudience"],
            claims: claims,
            notBefore: DateTime.Now,
            expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings["expires"])),
            signingCredentials: signingCredentials
        );
        return tokenOptions;
    }
}
