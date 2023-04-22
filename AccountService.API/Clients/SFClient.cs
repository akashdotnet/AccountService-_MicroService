using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using AccountService.API.Clients.Interfaces;
using AccountService.API.Constants;
using AccountService.API.Dto.Request;
using AccountService.API.Dto.SalesForce;
using AccountService.API.Utils.Extensions;
using Microsoft.Extensions.Configuration;
using PodCommonsLibrary.Core.Exceptions;
using PodCommonsLibrary.Core.Utils;

namespace AccountService.API.Clients;

public class SfClient : ISfClient
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly HttpClientUtility _httpClientUtility;

    public SfClient(HttpClient httpClient, IConfiguration configuration, HttpClientUtility httpClientUtility)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        if (_httpClient.BaseAddress == null)
        {
            throw new NotFoundException(StaticValues.SalesForceBaseAddressNotFound,
                StaticValues.ErrorSalesForceBaseAddressNotFound);
        }

        _configuration = configuration;
        _httpClientUtility = httpClientUtility;
    }

    public async Task<TokenResponse> GetToken(TokenRequest tokenRequest)
    {
        string callbackUrl = string.IsNullOrEmpty(tokenRequest.CallbackUrl)
            ? _configuration["SfRedirectUrl"]
            : tokenRequest.CallbackUrl;
        IList<KeyValuePair<string, string>> formContent = new List<KeyValuePair<string, string>>
        {
            new("code", HttpUtility.UrlDecode(tokenRequest.AuthCode)),
            new("client_id", _configuration["SfConnectedAppConsumerKey"]),
            new("client_secret", _configuration["SfConnectedAppConsumerSecret"]),
            new("redirect_uri", callbackUrl),
            new("grant_type", StaticValues.AuthCodeGrantType),
            new("format", "json")
        };
        Uri uriPath =
            new($"{_httpClient.BaseAddress}{_configuration["SfExperiencedSitePath"]}/{StaticValues.SfTokenPath}");
        return await GetOAuthTokens(uriPath, formContent);
    }

    public async Task<UserInformationResponse> GetUserInformation(TokenResponse tokenResponse)
    {
        Uri uriPath =
            new Uri($"{_httpClient.BaseAddress}{_configuration["SfExperiencedSitePath"]}/{StaticValues.UserInfoPath}")
                .AddQuery(StaticValues.SfAccessTokenQueryParameter, tokenResponse.AccessToken);

        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(uriPath);
        return await _httpClientUtility.PrepareResponse<UserInformationResponse>(httpResponseMessage, uriPath);
    }

    public async Task<TokenResponse> RefreshToken(RefreshTokenRequest refreshTokenRequest)
    {
        IList<KeyValuePair<string, string>> formContent = new List<KeyValuePair<string, string>>
        {
            new("refresh_token", HttpUtility.UrlDecode(refreshTokenRequest.RefreshToken)),
            new("client_id", _configuration["SfConnectedAppConsumerKey"]),
            new("client_secret", _configuration["SfConnectedAppConsumerSecret"]),
            new("grant_type", StaticValues.RefreshTokenGrantType)
        };
        Uri uriPath =
            new($"{_httpClient.BaseAddress}{_configuration["SfExperiencedSitePath"]}/{StaticValues.SfTokenPath}");
        return await GetOAuthTokens(uriPath, formContent);
    }

    public async Task RevokeToken(RefreshTokenRequest refreshTokenRequest)
    {
        Uri uriPath =
            new($"{_httpClient.BaseAddress}{_configuration["SfExperiencedSitePath"]}/{StaticValues.RevokeTokenPath}");
        IList<KeyValuePair<string, string>> formContent = new List<KeyValuePair<string, string>>
        {
            new("token", HttpUtility.UrlDecode(refreshTokenRequest.RefreshToken))
        };
        await GetOAuthTokens(uriPath, formContent);
    }

    private async Task<TokenResponse> GetOAuthTokens(Uri uriPath, IList<KeyValuePair<string, string>> formContent)
    {
        HttpResponseMessage httpResponseMessage =
            await _httpClient.PostAsync(uriPath, new FormUrlEncodedContent(formContent));
        return await _httpClientUtility.PrepareResponse<TokenResponse>(httpResponseMessage, uriPath);
    }
}
