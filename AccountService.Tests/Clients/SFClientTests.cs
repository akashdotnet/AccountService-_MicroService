using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AccountService.API.Clients;
using AccountService.API.Dto.Request;
using AccountService.API.Dto.SalesForce;
using AutoFixture;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using PodCommonsLibrary.Core.Dto;
using PodCommonsLibrary.Core.Exceptions;
using PodCommonsLibrary.Core.Utils;
using Xunit;

namespace AccountService.API.Tests.Clients;

public class SFClientTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly IFixture _fixture;

    public SFClientTests()
    {
        _fixture = new Fixture();
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.SetupGet(x =>
                x[It.Is<string>(s => s == "SfConnectedAppConsumerKey")])
            .Returns("abc");
        _configurationMock.SetupGet(x =>
                x[It.Is<string>(s => s == "SfConnectedAppConsumerSecret")])
            .Returns("abc");
        _configurationMock.SetupGet(x =>
                x[It.Is<string>(s => s == "SfRedirectUrl")])
            .Returns("abc");
    }

    private SfClient GetSfClientObject(HttpResponseMessage message)
    {
        Mock<HttpMessageHandler> handlerMock = new(MockBehavior.Loose);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            // prepare the expected response of the mocked http call
            .ReturnsAsync(message);

        // use real http client with mocked handler here
        HttpClient httpClient = new(handlerMock.Object)
        {
            BaseAddress = new Uri("https://test.com/")
        };
        HttpClientUtility httpClientUtility = new();
        return new SfClient(httpClient, _configurationMock.Object, httpClientUtility);
    }

    [Fact(DisplayName = "SfClient: Setup - Should throw NotFoundException when there is no base address.")]
    public void ClientSetup_Failure()
    {
        // arrange
        Mock<HttpMessageHandler> handlerMock = new(MockBehavior.Loose);
        HttpClient httpClient = new(handlerMock.Object)
        {
            BaseAddress = null
        };
        HttpClientUtility httpClientUtility = new();

        // assert
        NotFoundException exception = Assert.ThrowsAny<NotFoundException>(() =>
            new SfClient(httpClient, _configurationMock.Object, httpClientUtility));

        // assert
        Assert.Equal("The base address for SalesForce is not found!", exception.Message);
    }

    [Fact(DisplayName = "SfClient: Get token - Should return JWT tokens on success.")]
    public async Task GetToken_Success()
    {
        // arrange
        TokenRequest request = _fixture.Create<TokenRequest>();
        HttpResponseMessage httpResponseMessage = new()
        {
            StatusCode = HttpStatusCode.Accepted,
            Content = new StringContent(JsonConvert.SerializeObject(_fixture
                .Build<TokenResponse>()
                .With(x => x.AccessToken, "at")
                .Create()))
        };
        SfClient client = GetSfClientObject(httpResponseMessage);

        // act
        TokenResponse result = await client.GetToken(request);

        // assert
        Assert.NotNull(result);
        Assert.Equal("at", result.AccessToken);
    }

    [Fact(DisplayName = "SfClient: Get token - Should return bad request error object on failure.")]
    public async Task GetToken_Failure()
    {
        // arrange
        TokenRequest request = _fixture.Create<TokenRequest>();
        HttpResponseMessage httpResponseMessage = new()
        {
            StatusCode = HttpStatusCode.BadRequest,
            Content = new StringContent(JsonConvert.SerializeObject(_fixture
                .Build<ErrorResponse>()
                .With(x => x.Message, "error")
                .With(x => x.ErrorResponseType, "type")
                .Create()))
        };
        SfClient client = GetSfClientObject(httpResponseMessage);

        // act and assert
        await Assert.ThrowsAsync<HttpRequestException>(async () =>
            await client.GetToken(request));
    }

    [Fact(DisplayName = "SfClient: Refresh token - Should return JWT tokens on success.")]
    public async Task RefreshToken_Success()
    {
        // arrange
        RefreshTokenRequest request = _fixture.Create<RefreshTokenRequest>();
        HttpResponseMessage httpResponseMessage = new()
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonConvert.SerializeObject(_fixture
                .Build<TokenResponse>()
                .With(x => x.AccessToken, "at")
                .Create()))
        };
        SfClient client = GetSfClientObject(httpResponseMessage);

        // act
        TokenResponse result = await client.RefreshToken(request);

        // assert
        Assert.NotNull(result);
        Assert.Equal("at", result.AccessToken);
    }

    [Fact(DisplayName = "SfClient: Revoke token - Should call the SF revoke function.")]
    public async Task RevokeToken_Success()
    {
        // arrange
        RefreshTokenRequest request = _fixture.Create<RefreshTokenRequest>();
        HttpResponseMessage httpResponseMessage = new()
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonConvert.SerializeObject(_fixture
                .Build<TokenResponse>()
                .With(x => x.AccessToken, "at")
                .Create()))
        };
        SfClient client = GetSfClientObject(httpResponseMessage);

        // act
        await client.RevokeToken(request);
    }

    [Fact(DisplayName =
        "SfClient: Get User Information - Should return user info from salesforce such as email, first name and last name.")]
    public async Task GetUserInformation_Success()
    {
        // arrange
        TokenResponse request = _fixture.Create<TokenResponse>();
        HttpResponseMessage httpResponseMessage = new()
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonConvert.SerializeObject(_fixture
                .Build<UserInformationResponse>()
                .With(x => x.Email, "abc@mail.com")
                .With(x => x.FirstName, "abc")
                .With(x => x.LastName, "def")
                .Create()))
        };
        SfClient client = GetSfClientObject(httpResponseMessage);

        // act
        UserInformationResponse result = await client.GetUserInformation(request);

        // assert
        Assert.Equal("abc@mail.com", result.Email);
        Assert.Equal("abc", result.FirstName);
        Assert.Equal("def", result.LastName);
    }
}
