using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AccountService.API.Clients.Interfaces;
using AccountService.API.Constants;
using AccountService.API.Dto.Request;
using AccountService.API.Dto.Response;
using AccountService.API.Dto.SalesForce;
using AccountService.API.Models;
using AccountService.API.Services.Interfaces;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using PodCommonsLibrary.Core.Constants;
using PodCommonsLibrary.Core.Enums;
using PodCommonsLibrary.Core.Exceptions;

namespace AccountService.API.Services;

public class MerchantService : IMerchantService
{
    private readonly IAccountService _accountService;
    private readonly IConfiguration _configuration;
    private readonly IDealerService _dealerService;
    private readonly IExpertService _expertService;
    private readonly ISfClient _sfClient;

    public MerchantService(IConfiguration configuration, IDealerService dealerService, IAccountService accountService,
        IExpertService expertService, ISfClient sfClient)
    {
        _configuration = configuration;
        _dealerService = dealerService;
        _accountService = accountService;
        _expertService = expertService;
        _sfClient = sfClient;
    }

    public OAuthRedirectResponse CreateRedirectUrl(LoginRedirectRequest loginRedirectRequest)
    {
        Dictionary<string, string> param = new()
        {
            {"client_id", _configuration["SfConnectedAppConsumerKey"]},
            {"redirect_uri", loginRedirectRequest.CallbackUrl},
            {"response_type", "code"}
        };
        string baseUrl =
            $"{_configuration["SfBaseUrl"]}/{_configuration["SfExperiencedSitePath"]}/{StaticValues.SfAuthorizePath}";
        OAuthRedirectResponse oAuthRedirectResponse = new()
        {
            RedirectUrl = new Uri(QueryHelpers.AddQueryString(baseUrl, param!))
        };
        return oAuthRedirectResponse;
    }

    public async Task<TokenResponse> GetToken(TokenRequest tokenRequest)
    {
        TokenResponse tokenResponse = await _sfClient.GetToken(tokenRequest);
        UserInformationResponse userInformation = await _sfClient.GetUserInformation(tokenResponse);
        Account? existingAccount = await _accountService.GetAccount(userInformation.Email);
        if (existingAccount != null)
        {
            return tokenResponse;
        }

        if (!BaseStaticValues.UserTypeRoleMap.ContainsKey(userInformation.CustomAttributes.UserType))
        {
            throw new BusinessRuleViolationException(StaticValues.InvalidUserType,
                StaticValues.ErrorUserTypeNotSupported);
        }

        switch (BaseStaticValues.UserTypeRoleMap[userInformation.CustomAttributes.UserType])
        {
            case UserRoleEnum.Dealer:
                await _dealerService.CreateDealer(userInformation);
                break;
            case UserRoleEnum.Expert:
                await _expertService.CreateExpert(userInformation);
                break;
            case UserRoleEnum.Customer:
            default:
                throw new BusinessRuleViolationException(StaticValues.InvalidUserType,
                    StaticValues.ErrorUserTypeMappingNotFound);
        }

        return tokenResponse;
    }

    public async Task<TokenResponse> RefreshToken(RefreshTokenRequest refreshTokenRequest)
    {
        TokenResponse tokenResponse = await _sfClient.RefreshToken(refreshTokenRequest);
        tokenResponse.RefreshToken = refreshTokenRequest.RefreshToken;
        return tokenResponse;
    }

    public async Task RevokeToken(RefreshTokenRequest refreshTokenRequest)
    {
        await _sfClient.RevokeToken(refreshTokenRequest);
    }
}
