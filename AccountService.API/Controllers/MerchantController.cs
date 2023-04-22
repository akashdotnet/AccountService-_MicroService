using System.Threading.Tasks;
using AccountService.API.Constants;
using AccountService.API.Dto.Request;
using AccountService.API.Dto.Response;
using AccountService.API.Dto.SalesForce;
using AccountService.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PodCommonsLibrary.Core.Dto;

namespace AccountService.API.Controllers;

[ApiController]
[Route(StaticValues.MerchantRoutePrefix)]
public class MerchantController : ControllerBase
{
    private readonly IMerchantService _merchantService;

    public MerchantController(IMerchantService sfService)
    {
        _merchantService = sfService;
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    [HttpPost(StaticValues.LoginRedirectPath)]
    public ActionResult<OAuthRedirectResponse> LoginRedirect([FromBody] LoginRedirectRequest loginRedirectRequest)
    {
        OAuthRedirectResponse oAuthRedirectResponse = _merchantService.CreateRedirectUrl(loginRedirectRequest);
        return StatusCode(StatusCodes.Status200OK, oAuthRedirectResponse);
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    [HttpPost(StaticValues.LoginTokenPath)]
    public async Task<ActionResult<TokenResponse>> GetToken([FromBody] TokenRequest tokenRequest)
    {
        TokenResponse tokenResponse = await _merchantService.GetToken(tokenRequest);
        return StatusCode(StatusCodes.Status200OK, tokenResponse);
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    [HttpPost(StaticValues.RefreshTokenPath)]
    public async Task<ActionResult<TokenResponse>> RefreshToken([FromBody] RefreshTokenRequest refreshTokenRequest)
    {
        TokenResponse tokenResponse = await _merchantService.RefreshToken(refreshTokenRequest);
        return StatusCode(StatusCodes.Status200OK, tokenResponse);
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    [HttpPost(StaticValues.LogoutPath)]
    public async Task<ActionResult> LogoutUser([FromBody] RefreshTokenRequest refreshTokenRequest)
    {
        await _merchantService.RevokeToken(refreshTokenRequest);
        return StatusCode(StatusCodes.Status200OK);
    }
}
