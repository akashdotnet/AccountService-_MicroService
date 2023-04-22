using System.Threading.Tasks;
using AccountService.API.Models;
using AccountService.API.Repositories.Interfaces;
using AutoFixture;
using AutoFixture.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Moq;
using PodCommonsLibrary.Core.Exceptions;
using Xunit;

namespace AccountService.API.Tests.Services;

public class AccountServiceTests
{
    private readonly Mock<IAccountRepository> _accountRepositoryMock;

    private readonly API.Services.AccountService _accountService;
    private readonly IFixture _fixture;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;

    public AccountServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Customizations.Add(new RegularExpressionGenerator());
        _fixture.Customizations.Add(new RegularExpressionAttributeRelay());
        _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _accountRepositoryMock = new Mock<IAccountRepository>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        _accountService = new API.Services.AccountService(
            _accountRepositoryMock.Object,
            _httpContextAccessorMock.Object
        );
    }

    [Fact(DisplayName = "Get account Success")]
    public async Task GetAccount_Success()
    {
        // arrange
        Account account = _fixture.Build<Account>()
            .With(x => x.Email, "abc@mail.com")
            .Create();
        _accountRepositoryMock.Setup(a =>
            a.GetAccountByEmail(It.IsAny<string>())).ReturnsAsync(account);

        // act
        Account? expected = await _accountService.GetAccount("abc@gmail.com");

        // assert
        Assert.Equal("abc@mail.com", expected?.Email);
    }

    [Fact(DisplayName = "Get Account ID Success")]
    public async Task GetAccountId_Success()
    {
        // arrange
        Account account = _fixture.Build<Account>()
            .With(x => x.Email, "abc@mail.com")
            .With(x => x.Id, 1)
            .Create();
        IHeaderDictionary headers = new HeaderDictionary
        {
            {"x-user-id", "abc@mail.com"}
        };
        _accountRepositoryMock.Setup(a =>
            a.GetAccountByEmail(It.IsAny<string>())).ReturnsAsync(account);
        _httpContextAccessorMock.Setup(h => h.HttpContext!.Request.Headers)
            .Returns(headers);

        // act
        int expected = await _accountService.GetAccountId();

        // assert
        Assert.Equal(1, expected);
    }

    [Fact(DisplayName = "Get Account ID Failure: No email claim")]
    public async Task GetAccountId_NoEmailClaim()
    {
        // arrange
        Account account = _fixture.Build<Account>()
            .With(x => x.Email, "abc@mail.com")
            .With(x => x.Id, 1)
            .Create();
        IHeaderDictionary headers = new HeaderDictionary
        {
            {"x-user-id", ""}
        };
        _accountRepositoryMock.Setup(a =>
            a.GetAccountByEmail(It.IsAny<string>())).ReturnsAsync(account);
        _httpContextAccessorMock.Setup(h => h.HttpContext!.Request.Headers)
            .Returns(headers);

        // act and assert
        await Assert.ThrowsAsync<UnauthorizedException>(async () =>
            await _accountService.GetAccountId());
    }

    [Fact(DisplayName = "Get Account ID Failure: No account")]
    public async Task GetAccountId_NoAccount()
    {
        // arrange
        IHeaderDictionary headers = new HeaderDictionary
        {
            {"x-user-id", "abc"}
        };
        _accountRepositoryMock.Setup(a =>
            a.GetAccountByEmail(It.IsAny<string>())).ReturnsAsync((Account?) null);
        _httpContextAccessorMock.Setup(h => h.HttpContext!.Request.Headers)
            .Returns(headers);

        // act and assert
        await Assert.ThrowsAsync<BusinessRuleViolationException>(async () =>
            await _accountService.GetAccountId());
    }

    [Fact(DisplayName =
        "AccountService: GetAccountId - Should successfully get the account id for the given email address.")]
    public async Task GetAccountId_Success_WithEmailAsArgument()
    {
        // arrange
        IHeaderDictionary headers = new HeaderDictionary
        {
            {"x-user-id", "abc"}
        };
        const string email = "test@mail.com";
        Account accountMock = _fixture.Build<Account>()
            .With(x => x.Email, email)
            .With(x => x.Id, 1)
            .Create();
        _accountRepositoryMock.Setup(a =>
            a.GetAccountByEmail(email)).ReturnsAsync(accountMock);
        _httpContextAccessorMock.Setup(h => h.HttpContext!.Request.Headers)
            .Returns(headers);

        // act 
        int result = await _accountService.GetAccountId(email);
        Assert.Equal(1, result);
        _accountRepositoryMock.Verify(a =>
            a.GetAccountByEmail(email), Times.Once);
        _httpContextAccessorMock.Verify(h =>
            h.HttpContext!.Request.Headers, Times.Never);
    }
}
