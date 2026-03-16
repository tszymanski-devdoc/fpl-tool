using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using FplTool.Modules.Auth.Contracts;
using FplTool.Modules.Auth.Domain;
using FplTool.Modules.Auth.Infrastructure;
using FplTool.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FplTool.Modules.Auth.Features.VerifyGoogleToken;

internal sealed class VerifyGoogleTokenHandler : IRequestHandler<VerifyGoogleTokenCommand, Result<AuthResponseDto>>
{
    private readonly AuthDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public VerifyGoogleTokenHandler(
        AuthDbContext dbContext,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<Result<AuthResponseDto>> Handle(VerifyGoogleTokenCommand request, CancellationToken cancellationToken)
    {
        var googleUser = await VerifyGoogleIdTokenAsync(request.IdToken, cancellationToken);
        if (googleUser is null)
            return Result.Failure<AuthResponseDto>(Error.Unauthorized("Invalid Google ID token."));

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.GoogleSubjectId == googleUser.Sub, cancellationToken);

        if (user is null)
        {
            user = User.Create(googleUser.Sub, googleUser.Email, googleUser.Name ?? googleUser.Email);
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var accessToken = GenerateJwt(user);

        return Result.Success(new AuthResponseDto(
            accessToken,
            new UserDto(user.Id, user.Email, user.DisplayName, user.FplManagerId)
        ));
    }

    private async Task<GoogleTokenInfo?> VerifyGoogleIdTokenAsync(string idToken, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient();
        try
        {
            var response = await client.GetAsync(
                $"https://oauth2.googleapis.com/tokeninfo?id_token={Uri.EscapeDataString(idToken)}", ct);

            if (!response.IsSuccessStatusCode)
                return null;

            var tokenInfo = await response.Content.ReadFromJsonAsync<GoogleTokenInfo>(ct);

            var clientId = _configuration["Google:ClientId"];
            if (!string.IsNullOrEmpty(clientId) && tokenInfo?.Aud != clientId)
                return null;

            return tokenInfo;
        }
        catch
        {
            return null;
        }
    }

    private string GenerateJwt(User user)
    {
        var signingKey = _configuration["Jwt:SigningKey"]
                         ?? throw new InvalidOperationException("JWT signing key is not configured.");
        var issuer = _configuration["Jwt:Issuer"] ?? "fpl-tool";
        var audience = _configuration["Jwt:Audience"] ?? "fpl-tool-client";
        var expiryMinutes = int.TryParse(_configuration["Jwt:ExpiryMinutes"], out var mins) ? mins : 1440;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("display_name", user.DisplayName)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

internal sealed class GoogleTokenInfo
{
    [JsonPropertyName("sub")]
    public string Sub { get; init; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("aud")]
    public string Aud { get; init; } = string.Empty;
}
