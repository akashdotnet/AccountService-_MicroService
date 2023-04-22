using System.Threading.Tasks;
using AccountService.API.Clients.Interfaces;
using AccountService.API.Dto.Request;
using AccountService.API.Dto.Response;
using AccountService.API.Dto.SalesForce;
using AccountService.API.Models;
using AccountService.API.Services;
using AccountService.API.Services.Interfaces;
using AutoFixture;
using AutoFixture.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Moq;
using PodCommonsLibrary.Core.Exceptions;
using Xunit;

namespace AccountService.API.Tests.Services;

public class MerchantServiceTests
{
    private readonly Mock<IAccountService> _accountServiceMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IDealerService> _dealerServiceMock;
    private readonly Mock<IExpertService> _expertServiceMock;
    private readonly IFixture _fixture;

    private readonly MerchantService _merchantService;
    private readonly Mock<ISfClient> _sfClientMock;

    public MerchantServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Customizations.Add(new RegularExpressionGenerator());
        _fixture.Customizations.Add(new RegularExpressionAttributeRelay());
        _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _accountServiceMock = new Mock<IAccountService>();
        _configurationMock = new Mock<IConfiguration>();
        _dealerServiceMock = new Mock<IDealerService>();
        _expertServiceMock = new Mock<IExpertService>();
        _sfClientMock = new Mock<ISfClient>();

        _merchantService = new MerchantService(
            _configurationMock.Object,
            _dealerServiceMock.Object,
            _accountServiceMock.Object,
            _expertServiceMock.Object,
            _sfClientMock.Object
        );
    }

    [Fact(DisplayName =
        "Merchant service: Create redirect url - Should append query params and return a SF redirect url.")]
    public async Task CreateRedirectUrl_Success()
    {
        // arrange
        LoginRedirectRequest mockRequest = _fixture.Build<LoginRedirectRequest>()
            .With(x => x.CallbackUrl, "http://url")
            .Create();
        _configurationMock.SetupGet(x =>
                x[It.Is<string>(s => s == "SfBaseUrl")])
            .Returns("http://base");
        _configurationMock.SetupGet(x =>
                x[It.Is<string>(s => s == "SfConnectedAppConsumerKey")])
            .Returns("abc");
        _configurationMock.SetupGet(x =>
                x[It.Is<string>(s => s == "SfExperiencedSitePath")])
            .Returns("merchant");

        // act
        OAuthRedirectResponse actualOutput = _merchantService.CreateRedirectUrl(mockRequest);

        // assert
        Assert.Equal(
            "http://base/merchant/services/oauth2/authorize?client_id=abc&redirect_uri=http%3A%2F%2Furl&response_type=code",
            actualOutput.RedirectUrl.ToString());
    }


    [Fact(DisplayName = "Merchant service: Get token - Should return JWT token when login is successful for dealer.")]
    public async Task GetToken_SuccessDealer()
    {
        // arrange
        TokenRequest mockRequest = _fixture.Build<TokenRequest>()
            .With(x => x.AuthCode, "token")
            .Create();
        CustomAttributes attributes = new()
        {
            UserType = "1"
        };
        _sfClientMock.Setup(x => x.GetToken(It.IsAny<TokenRequest>()))
            .ReturnsAsync(_fixture.Build<TokenResponse>()
                .With(x => x.AccessToken, "mock_at_1")
                .Create());
        _sfClientMock.Setup(x => x.GetUserInformation(It.IsAny<TokenResponse>()))
            .ReturnsAsync(_fixture.Build<UserInformationResponse>()
                .With(x => x.CustomAttributes, attributes)
                .Create());
        _accountServiceMock.Setup(x => x.GetAccount(It.IsAny<string>()))
            .ReturnsAsync((Account?) null);
        _dealerServiceMock.Setup(x => x.CreateDealer(It.IsAny<UserInformationResponse>()))
            .ReturnsAsync(_fixture.Create<DealerResponse>());

        // act
        TokenResponse actualOutput = await _merchantService.GetToken(mockRequest);

        // assert
        Assert.Equal("mock_at_1", actualOutput.AccessToken);
    }

    [Fact(DisplayName = "Merchant service: Get token - Should return JWT token when login is successful for expert.")]
    public async Task GetToken_SuccessExpert()
    {
        // arrange
        TokenRequest mockRequest = _fixture.Build<TokenRequest>()
            .With(x => x.AuthCode, "token")
            .Create();
        CustomAttributes attributes = new()
        {
            UserType = "2"
        };
        _sfClientMock.Setup(x => x.GetToken(It.IsAny<TokenRequest>()))
            .ReturnsAsync(_fixture.Build<TokenResponse>()
                .With(x => x.AccessToken, "mock_at_2")
                .Create());
        _sfClientMock.Setup(x => x.GetUserInformation(It.IsAny<TokenResponse>()))
            .ReturnsAsync(_fixture.Build<UserInformationResponse>()
                .With(x => x.CustomAttributes, attributes)
                .Create());
        _accountServiceMock.Setup(x => x.GetAccount(It.IsAny<string>()))
            .ReturnsAsync((Account?) null);
        _expertServiceMock.Setup(x => x.CreateExpert(It.IsAny<UserInformationResponse>()))
            .ReturnsAsync(_fixture.Create<ExpertResponse>());

        // act
        TokenResponse actualOutput = await _merchantService.GetToken(mockRequest);

        // assert
        Assert.Equal("mock_at_2", actualOutput.AccessToken);
    }

    [Fact(DisplayName = "Merchant service: Get token - Should return tokens immediately when account exists.")]
    public async Task GetToken_SuccessExistingAccount()
    {
        // arrange
        TokenRequest mockRequest = _fixture.Build<TokenRequest>()
            .With(x => x.AuthCode, "token")
            .Create();
        _sfClientMock.Setup(x => x.GetToken(It.IsAny<TokenRequest>()))
            .ReturnsAsync(_fixture.Build<TokenResponse>()
                .With(x => x.AccessToken, "mock_at_3")
                .Create());
        _sfClientMock.Setup(x => x.GetUserInformation(It.IsAny<TokenResponse>()))
            .ReturnsAsync(_fixture.Create<UserInformationResponse>());
        _accountServiceMock.Setup(x => x.GetAccount(It.IsAny<string>()))
            .ReturnsAsync(_fixture.Create<Account>());

        // act
        TokenResponse actualOutput = await _merchantService.GetToken(mockRequest);

        // assert
        Assert.Equal("mock_at_3", actualOutput.AccessToken);
    }

    [Fact(DisplayName = "Merchant service: Get token - Should throw error when profile is customer")]
    public async Task GetToken_FailureCustomerProfile()
    {
        // arrange
        TokenRequest mockRequest = _fixture.Build<TokenRequest>()
            .With(x => x.AuthCode, "token")
            .Create();
        CustomAttributes attributes = new()
        {
            UserType = "Customer"
        };
        _sfClientMock.Setup(x => x.GetToken(It.IsAny<TokenRequest>()))
            .ReturnsAsync(_fixture.Build<TokenResponse>()
                .With(x => x.AccessToken, "mock_at_3")
                .Create());
        _sfClientMock.Setup(x => x.GetUserInformation(It.IsAny<TokenResponse>()))
            .ReturnsAsync(_fixture.Build<UserInformationResponse>()
                .With(x => x.CustomAttributes, attributes)
                .Create());
        _accountServiceMock.Setup(x => x.GetAccount(It.IsAny<string>()))
            .ReturnsAsync((Account?) null);

        // act
        BusinessRuleViolationException exception = await Assert.ThrowsAsync<BusinessRuleViolationException>(async () =>
            await _merchantService.GetToken(mockRequest));

        // assert
        Assert.Equal("The mapping for the given user type doesn't exist!", exception.Message);
    }

    [Fact(DisplayName = "Merchant service: Get token - Should throw error when profile is not supported")]
    public async Task GetToken_FailureUnsupportedProfile()
    {
        // arrange
        TokenRequest mockRequest = _fixture.Build<TokenRequest>()
            .With(x => x.AuthCode, "token")
            .Create();
        CustomAttributes attributes = new()
        {
            UserType = "Unsupported Profile"
        };
        _sfClientMock.Setup(x => x.GetToken(It.IsAny<TokenRequest>()))
            .ReturnsAsync(_fixture.Build<TokenResponse>()
                .With(x => x.AccessToken, "mock_at_3")
                .Create());
        _sfClientMock.Setup(x => x.GetUserInformation(It.IsAny<TokenResponse>()))
            .ReturnsAsync(_fixture.Build<UserInformationResponse>()
                .With(x => x.CustomAttributes, attributes)
                .Create());
        _accountServiceMock.Setup(x => x.GetAccount(It.IsAny<string>()))
            .ReturnsAsync((Account?) null);

        // act
        BusinessRuleViolationException exception = await Assert.ThrowsAsync<BusinessRuleViolationException>(async () =>
            await _merchantService.GetToken(mockRequest));

        // assert
        Assert.Equal("We only support dealer and expert user types!", exception.Message);
    }

    [Fact(DisplayName = "Merchant service: Refresh token - Should return JWT tokens on success.")]
    public async Task RefreshToken_Success()
    {
        // arrange
        RefreshTokenRequest mockRequest = _fixture.Build<RefreshTokenRequest>()
            .With(x => x.RefreshToken, "mock_rt")
            .Create();
        _sfClientMock.Setup(x => x.RefreshToken(It.IsAny<RefreshTokenRequest>()))
            .ReturnsAsync(_fixture.Build<TokenResponse>()
                .With(x => x.AccessToken, "mock_at")
                .Create());
        // act
        TokenResponse actualOutput = await _merchantService.RefreshToken(mockRequest);

        // assert
        Assert.Equal("mock_at", actualOutput.AccessToken);
        Assert.Equal("mock_rt", actualOutput.RefreshToken);
    }

    [Fact(DisplayName = "Merchant service: Revoke token - Should return JWT tokens on success.")]
    public async Task RevokeToken()
    {
        // arrange
        RefreshTokenRequest mockRequest = _fixture.Build<RefreshTokenRequest>()
            .With(x => x.RefreshToken, "mock_rt")
            .Create();
        _sfClientMock.Setup(x => x.RevokeToken(It.IsAny<RefreshTokenRequest>()))
            .Returns(Task.FromResult(true));
        // act
        await _merchantService.RevokeToken(mockRequest);

        // assert
        _sfClientMock.Verify(x => x.RevokeToken(It.IsAny<RefreshTokenRequest>()), Times.Once());
    }
}
