using System.Threading.Tasks;
using AccountService.API.Dto.Request;
using AccountService.API.Dto.Response;
using AccountService.API.Dto.SalesForce;

namespace AccountService.API.Services.Interfaces;

public interface IMerchantService
{
    OAuthRedirectResponse CreateRedirectUrl(LoginRedirectRequest loginRedirectRequest);

    Task<TokenResponse> GetToken(TokenRequest tokenRequest);
    Task<TokenResponse> RefreshToken(RefreshTokenRequest refreshTokenRequest);
    Task RevokeToken(RefreshTokenRequest refreshTokenRequest);
}
