using System.Threading.Tasks;
using AccountService.API.Dto.Request;
using AccountService.API.Dto.SalesForce;

namespace AccountService.API.Clients.Interfaces;

public interface ISfClient
{
    Task<TokenResponse> GetToken(TokenRequest tokenRequest);
    Task<UserInformationResponse> GetUserInformation(TokenResponse tokenResponse);
    Task<TokenResponse> RefreshToken(RefreshTokenRequest refreshTokenRequest);
    Task RevokeToken(RefreshTokenRequest refreshTokenRequest);
}
