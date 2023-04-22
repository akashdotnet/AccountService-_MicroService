using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AccountService.API.Config;

public class TokenConfig
{
    private readonly IConfiguration _configuration;

    public TokenConfig(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetJwtToken(ClaimsIdentity claimsIdentity)
    {
        byte[] key = Convert.FromBase64String(_configuration["JwtSecret"]);
        JwtSecurityTokenHandler tokenHandler = new();
        SecurityTokenDescriptor tokenDescriptor = new()
        {
            Subject = claimsIdentity,
            Expires = DateTime.UtcNow.AddDays(1),
            Issuer = _configuration["JwtIssuer"],
            Audience = _configuration["JwtAudience"],
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        SecurityToken? token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GetRefreshToken()
    {
        byte[] randomNumber = new byte[32];
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
