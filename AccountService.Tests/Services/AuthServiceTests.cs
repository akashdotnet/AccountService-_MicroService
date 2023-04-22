using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AccountService.API.Config;
using AccountService.API.Constants;
using AccountService.API.Dto;
using AccountService.API.Dto.Request;
using AccountService.API.Dto.Response;
using AccountService.API.Models;
using AccountService.API.Repositories.Interfaces;
using AccountService.API.Services;
using AccountService.API.Services.Interfaces;
using AccountService.API.Utils.Extensions;
using AutoFixture;
using AutoFixture.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Moq;
using PodCommonsLibrary.Core.Enums;
using PodCommonsLibrary.Core.Exceptions;
using Xunit;

namespace AccountService.API.Tests.Services;

public class AuthServiceTests
{
    private const string mockJwtSecret =
        "Y3BrYmVjd3Rvd2p3d2lhdWJlY3dvdG5iamZxYnh2bW54Z2hsbW1vamh1bmprcHpib2hsZm9kdmpoYmN2aGJqaHZwdW5ubGx4d3N3dXZpZ2lwZ2J4ZnVwZ2Nud2FyZWRobnBzb2hmcWJnaXJjbnlya3B0eWV4eG9seXdxY2R3cWhtCg==";

    private const string mockJwtToken =
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6InZpQGdtYWlsLmNvbSIsInJvbGUiOiJDdXN0b21lciIsIm5iZiI6MTY0NzM2NDkzMywiZXhwIjoxNjQ3NDUxMzMzLCJpYXQiOjE2NDczNjQ5MzMsImlzcyI6InBudC1pc3NcbiIsImF1ZCI6InBudC1hdWRcbiJ9.kew1X6LsczmIgdJixrtFZccdU-F_MhkSB7RKK9PyJEA";

    private readonly Mock<IAccountService> _accountServiceMock;
    private readonly AuthService _authService;
    private readonly Mock<IDistributedCache> _cache;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly IFixture _fixture;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<DistributedCacheEntryOptions> _mockDistributedCacheEntryOptions;
    private readonly Mock<SignInManager<IdentityUser>> _signInManager;
    private readonly Mock<TokenConfig> _tokenConfig;
    private readonly Mock<UserManager<IdentityUser>> _userManager;
    private readonly Mock<IConfiguration> mockConfig;

    public AuthServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Customizations.Add(new RegularExpressionGenerator());
        _fixture.Customizations.Add(new RegularExpressionAttributeRelay());
        _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _accountServiceMock = new Mock<IAccountService>();
        _cache = new Mock<IDistributedCache>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _userManager = new Mock<UserManager<IdentityUser>>(Mock.Of<IUserStore<IdentityUser>>(),
            null, null, null, null, null, null, null, null);
        _signInManager = new Mock<SignInManager<IdentityUser>>(
            _userManager.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<IdentityUser>>(),
            null, null, null, null);
        mockConfig = new Mock<IConfiguration>();
        mockConfig.SetupGet(x => x[It.Is<string>(s => s == "JwtIssuer")]).Returns("mock");
        mockConfig.SetupGet(x => x[It.Is<string>(s => s == "JwtAudience")]).Returns("mock");
        mockConfig.SetupGet(x => x[It.Is<string>(s => s == "JwtSecret")]).Returns(mockJwtSecret);
        _tokenConfig = new Mock<TokenConfig>(mockConfig.Object);
        _mockDistributedCacheEntryOptions = new Mock<DistributedCacheEntryOptions>();

        _authService = new AuthService(
            _userManager.Object,
            _signInManager.Object,
            _tokenConfig.Object,
            _cache.Object,
            _accountServiceMock.Object,
            _httpContextAccessorMock.Object,
            _customerRepositoryMock.Object
        );
        _cache.Setup(x =>
                x.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), _mockDistributedCacheEntryOptions.Object, default))
            .Returns(Task.FromResult(true));
    }

    [Fact(DisplayName = "Generate Reset Token Success")]
    public async Task GenerateResetToken_Success()
    {
        // arrange
        CustomerResetPasswordRequest customerResetPasswordRequest = _fixture.Create<CustomerResetPasswordRequest>();
        customerResetPasswordRequest.Email = "abc@mail.com";
        IdentityUser mockUser = _fixture.Create<IdentityUser>();
        mockUser.Email = "abc@mail.com";
        _userManager.Setup(u => u
                .FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(mockUser);
        // act
        Session actualOutput = await _authService.GenerateResetToken(customerResetPasswordRequest);

        // assert
        Guid guidResult;
        Assert.True(Guid.TryParse(actualOutput.SessionId, out guidResult));
    }

    [Fact(DisplayName = "Generate Reset Token Failure: No user found")]
    public async Task GenerateResetToken_UserNotFound()
    {
        // arrange
        CustomerResetPasswordRequest customerResetPasswordRequest = _fixture.Create<CustomerResetPasswordRequest>();
        _userManager.Setup(u => u
                .FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((IdentityUser) null);
        // act
        BusinessRuleViolationException businessRuleViolationException =
            await Assert.ThrowsAsync<BusinessRuleViolationException>(async () =>
                await _authService.GenerateResetToken(customerResetPasswordRequest));

        // assert
        Assert.Equal("Reset token failure!",
            businessRuleViolationException.Message);
    }

    [Fact(DisplayName = "Set New Password Success")]
    public async Task SetNewPassword_Success()
    {
        // arrange
        CustomerSetNewPassword setNewPassword = _fixture.Create<CustomerSetNewPassword>();
        setNewPassword.Token = "abcd";
        setNewPassword.Password = "pass";
        Customer mockCustomer = _fixture.Create<Customer>();
        IdentityUser mockUser = _fixture.Build<IdentityUser>()
            .With(x => x.Email, "abc@mail.com")
            .Create();
        CustomerSession mockSession = _fixture.Build<CustomerSession>()
            .With(x => x.OtpVerified, true)
            .With(x => x.Email, "abc@mail.com")
            .Create();

        _cache.Setup(c => c
                .GetAsync(It.IsAny<string>(), default))
            .ReturnsAsync(mockSession.ObjectToByteArray());
        _cache.Setup(c => c
                .RemoveAsync(It.IsAny<string>(), default))
            .Returns(Task.FromResult(true));

        _userManager.Setup(u => u
                .FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(mockUser);
        _userManager.Setup(u => u
                .GeneratePasswordResetTokenAsync(It.IsAny<IdentityUser>()))
            .ReturnsAsync("mock");

        _userManager.Setup(u => u
                .ResetPasswordAsync(It.IsAny<IdentityUser>(),
                    It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _customerRepositoryMock.Setup(x => x.UpdateCustomer(It.IsAny<Customer>())).ReturnsAsync(mockCustomer);
        _customerRepositoryMock.Setup(x => x.GetCustomerByEmail(mockSession.Email)).ReturnsAsync(mockCustomer);
        // act
        IdentityResult result = await _authService.SetNewPassword(setNewPassword);

        // assert
        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.Equal(mockCustomer.PasswordResetDate, DateTime.UtcNow.Date);
    }

    [Fact(DisplayName = "Set New Password Failure: No token present in cache")]
    public async Task SetNewPassword_NoCacheKey()
    {
        // arrange
        CustomerSetNewPassword setNewPassword = _fixture.Create<CustomerSetNewPassword>();
        setNewPassword.Token = "abcd";
        setNewPassword.Password = "pass";
        IdentityUser mockUser = _fixture.Create<IdentityUser>();
        mockUser.Email = "abc@mail.com";

        _cache.Setup(c => c
                .GetAsync(It.IsAny<string>(), default))
            .ReturnsAsync((byte[]) null);

        UnauthorizedException unauthorizedException =
            await Assert.ThrowsAsync<UnauthorizedException>(async () =>
                await _authService.SetNewPassword(setNewPassword));

        // assert
        Assert.Equal("Refresh token is invalid!",
            unauthorizedException.Message);
    }

    [Fact(DisplayName = "Set New Password Failure: Identity set password failure")]
    public async Task SetNewPassword_IdentityFailure()
    {
        // arrange
        CustomerSetNewPassword setNewPassword = _fixture.Create<CustomerSetNewPassword>();
        setNewPassword.Token = "abcd";
        setNewPassword.Password = "pass";
        IdentityUser mockUser = _fixture.Build<IdentityUser>()
            .With(x => x.Email, "abc@mail.com")
            .Create();
        CustomerSession mockSession = _fixture.Build<CustomerSession>()
            .With(x => x.OtpVerified, true)
            .With(x => x.Email, "abc@mail.com")
            .Create();

        IdentityResult? mockResult = IdentityResult.Failed();
        _cache.Setup(c => c
                .GetAsync(It.IsAny<string>(), default))
            .ReturnsAsync(mockSession.ObjectToByteArray());
        _cache.Setup(c => c
                .RemoveAsync(It.IsAny<string>(), default))
            .Returns(Task.FromResult(true));

        _userManager.Setup(u => u
                .FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(mockUser);
        _userManager.Setup(u => u
                .GeneratePasswordResetTokenAsync(It.IsAny<IdentityUser>()))
            .ReturnsAsync("mock");
        _userManager.Setup(u => u
                .ResetPasswordAsync(It.IsAny<IdentityUser>(),
                    It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(mockResult);

        // act and assert
        await Assert.ThrowsAsync<UnauthorizedException>(async () =>
            await _authService.SetNewPassword(setNewPassword));
    }

    [Fact(DisplayName = "Set New Password Failure: OTP not verified")]
    public async Task SetNewPassword_OtpNotVerified()
    {
        // arrange
        CustomerSetNewPassword setNewPassword = _fixture.Create<CustomerSetNewPassword>();
        setNewPassword.Token = "abcd";
        setNewPassword.Password = "pass";
        IdentityUser mockUser = _fixture.Build<IdentityUser>()
            .With(x => x.Email, "abc@mail.com")
            .Create();
        CustomerSession mockSession = _fixture.Build<CustomerSession>()
            .With(x => x.OtpVerified, false)
            .With(x => x.Email, "abc@mail.com")
            .Create();

        IdentityResult? mockResult = IdentityResult.Failed();
        _cache.Setup(c => c
                .GetAsync(It.IsAny<string>(), default))
            .ReturnsAsync(mockSession.ObjectToByteArray());
        _cache.Setup(c => c
                .RemoveAsync(It.IsAny<string>(), default))
            .Returns(Task.FromResult(true));

        _userManager.Setup(u => u
                .FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(mockUser);
        _userManager.Setup(u => u
                .ResetPasswordAsync(It.IsAny<IdentityUser>(),
                    It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(mockResult);

        // act and assert
        UnauthorizedException exception = await Assert.ThrowsAsync<UnauthorizedException>(async () =>
            await _authService.SetNewPassword(setNewPassword));

        Assert.Equal("Refresh token OTP not verified!",
            exception.Message);
    }

    [Fact(DisplayName = "Create Identity User Success")]
    public async Task CreateIdentityUser_Success()
    {
        // arrange
        BaseUserRegistrationRequest request = _fixture.Build<BaseUserRegistrationRequest>()
            .With(x => x.Email, "abc@mail.com")
            .Create();

        _userManager.Setup(u => u
                .FindByNameAsync(It.IsAny<string>()))!
            .ReturnsAsync((IdentityUser) null);
        _userManager.Setup(u => u
                .CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(u => u
                .AddToRoleAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        // act
        IdentityResult? result = await _authService.CreateIdentityUser(request, UserRoleEnum.Customer);

        // assert
        Assert.NotNull(result);
        Assert.True(result.Succeeded);
    }

    [Fact(DisplayName = "Create Identity User Failure: User already exists")]
    public async Task CreateIdentityUser_UserExists()
    {
        // arrange
        BaseUserRegistrationRequest request = _fixture.Build<BaseUserRegistrationRequest>()
            .With(x => x.Email, "abc@mail.com")
            .Create();
        IdentityUser mockUser = _fixture.Build<IdentityUser>()
            .With(x => x.Email, "abc@mail.com")
            .Create();
        _userManager.Setup(u => u
                .FindByNameAsync(It.IsAny<string>()))!
            .ReturnsAsync(mockUser);

        // act
        BusinessRuleViolationException businessRuleViolationException =
            await Assert.ThrowsAsync<BusinessRuleViolationException>(async () =>
                await _authService.CreateIdentityUser(request, UserRoleEnum.Customer));

        // assert
        Assert.Equal("User registration error!",
            businessRuleViolationException.Message);
    }

    [Fact(DisplayName = "Create Identity User Failure: Identity failure")]
    public async Task CreateIdentityUser_IdentityFailure()
    {
        // arrange
        BaseUserRegistrationRequest request = _fixture.Build<BaseUserRegistrationRequest>()
            .With(x => x.Email, "abc@mail.com")
            .Create();
        IdentityError error = new()
        {
            Code = "User creation failed",
            Description = "User creation failed"
        };
        IdentityResult? mockResult = IdentityResult.Failed(error);
        _userManager.Setup(u => u
                .FindByNameAsync(It.IsAny<string>()))!
            .ReturnsAsync((IdentityUser) null);
        _userManager.Setup(u => u
                .CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(mockResult);

        // act
        BusinessRuleViolationException businessRuleViolationException =
            await Assert.ThrowsAsync<BusinessRuleViolationException>(async () =>
                await _authService.CreateIdentityUser(request, UserRoleEnum.Customer));

        // assert
        Assert.Equal("User creation failed",
            businessRuleViolationException.Message);
    }

    [Fact(DisplayName = "Generate Token Success")]
    public async Task GenerateToken_Success()
    {
        // arrange
        LoginRequest request = _fixture.Build<LoginRequest>()
            .With(x => x.Email, "abc@mail.com")
            .Create();
        IdentityUser mockUser = _fixture.Build<IdentityUser>()
            .With(x => x.Email, "abc@mail.com")
            .Create();
        Account account = _fixture.Build<Account>()
            .With(x => x.Email)
            .Create();
        IList<string>? userRoles = new List<string> {"abc"};

        _userManager.Setup(u => u
                .FindByNameAsync(It.IsAny<string>()))!
            .ReturnsAsync(mockUser);
        _accountServiceMock.Setup(u => u
                .GetAccount(It.IsAny<string>()))
            .ReturnsAsync(account);
        _signInManager.Setup(u => u
                .PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(SignInResult.Success);
        _userManager.Setup(u => u
                .GetRolesAsync(It.IsAny<IdentityUser>()))
            .ReturnsAsync(userRoles);

        // act
        LoginResponse result = await _authService.GenerateToken(request);

        // assert
        Assert.NotNull(result);
        Assert.StartsWith("ey", result.AccessToken);
        Assert.Equal(44, result.RefreshToken.Length);
    }

    [Fact(DisplayName = "Generate Token Failure: Incorrect credentials")]
    public async Task GenerateToken_IncorrectCredentials()
    {
        // arrange
        LoginRequest request = _fixture.Build<LoginRequest>()
            .With(x => x.Email, "abc@mail.com")
            .Create();
        IdentityUser mockUser = _fixture.Build<IdentityUser>()
            .With(x => x.Email, "abc@mail.com")
            .Create();
        Account account = _fixture.Build<Account>()
            .With(x => x.Email)
            .Create();

        _userManager.Setup(u => u
                .FindByNameAsync(It.IsAny<string>()))!
            .ReturnsAsync(mockUser);
        _accountServiceMock.Setup(u => u
                .GetAccount(It.IsAny<string>()))
            .ReturnsAsync(account);
        _signInManager.Setup(u => u
                .PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(SignInResult.Failed);

        // act and assert
        await Assert.ThrowsAsync<UnauthorizedException>(async () =>
            await _authService.GenerateToken(request));
    }

    [Fact(DisplayName = "Generate Token Failure: User not found")]
    public async Task GenerateToken_UserNotFound()
    {
        // arrange
        LoginRequest request = _fixture.Build<LoginRequest>()
            .With(x => x.Email, "abc@mail.com")
            .Create();
        IdentityUser mockUser = _fixture.Build<IdentityUser>()
            .With(x => x.Email, "abc@mail.com")
            .Create();

        _userManager.Setup(u => u
                .FindByNameAsync(It.IsAny<string>()))!
            .ReturnsAsync(mockUser);
        _accountServiceMock.Setup(u => u
                .GetAccount(It.IsAny<string>()))
            .ReturnsAsync((Account) null);

        // act and assert
        await Assert.ThrowsAsync<BusinessRuleViolationException>(async () =>
            await _authService.GenerateToken(request));
    }

    [Fact(DisplayName = "Refresh Token Success")]
    public async Task RefreshToken_Success()
    {
        // arrange
        RefreshTokenRequest request = _fixture.Build<RefreshTokenRequest>()
            .With(x => x.AccessToken, mockJwtToken)
            .With(x => x.RefreshToken, "refresh_token")
            .Create();
        IdentityUser mockUser = _fixture.Build<IdentityUser>()
            .With(x => x.Email, "abc@mail.com")
            .Create();
        Account account = _fixture.Build<Account>()
            .With(x => x.Email)
            .Create();
        IList<string>? userRoles = new List<string> {"abc"};

        _userManager.Setup(u => u
                .FindByNameAsync(It.IsAny<string>()))!
            .ReturnsAsync(mockUser);
        _userManager.Setup(u => u
                .GetRolesAsync(It.IsAny<IdentityUser>()))
            .ReturnsAsync(userRoles);
        _accountServiceMock.Setup(u => u
                .GetAccount(It.IsAny<string>()))
            .ReturnsAsync(account);
        _cache.Setup(c => c
                .GetAsync(It.IsAny<string>(), default))
            .ReturnsAsync(Encoding.ASCII.GetBytes("refresh_token"));

        // act
        LoginResponse response = await _authService.RefreshToken(request);

        // assert
        Assert.Equal(44, response.RefreshToken.Length);
    }

    [Fact(DisplayName = "Refresh Token Failure")]
    public async Task RefreshToken_Failure()
    {
        // arrange
        RefreshTokenRequest request = _fixture.Build<RefreshTokenRequest>()
            .With(x => x.AccessToken, mockJwtToken)
            .With(x => x.RefreshToken, "refresh_token")
            .Create();
        IdentityUser mockUser = _fixture.Build<IdentityUser>()
            .With(x => x.Email, "abc@mail.com")
            .Create();
        Account account = _fixture.Build<Account>()
            .With(x => x.Email)
            .Create();
        IList<string>? userRoles = new List<string> {"abc"};

        _userManager.Setup(u => u
                .FindByNameAsync(It.IsAny<string>()))!
            .ReturnsAsync(mockUser);
        _userManager.Setup(u => u
                .GetRolesAsync(It.IsAny<IdentityUser>()))
            .ReturnsAsync(userRoles);
        _accountServiceMock.Setup(u => u
                .GetAccount(It.IsAny<string>()))
            .ReturnsAsync(account);
        _cache.Setup(c => c
                .GetAsync(It.IsAny<string>(), default))
            .ReturnsAsync(Encoding.ASCII.GetBytes("refresh_token_fail"));

        await Assert.ThrowsAsync<UnauthorizedException>(async () =>
            await _authService.RefreshToken(request));
    }

    [Fact(DisplayName = "AuthService: CheckIfUserExists - throws business validation if user already exists.")]
    public async Task CheckIfUserExists_WhenUserAlreadyExists_ThrowBusinessValidation()
    {
        IdentityUser identityUser = _fixture.Create<IdentityUser>();
        _userManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(identityUser);

        BusinessRuleViolationException exception =
            await Assert.ThrowsAsync<BusinessRuleViolationException>(async () =>
                await _authService.CheckIfUserExists(It.IsAny<string>()));
        Assert.Equal(StaticValues.UserWithSameEmailExists, exception.ErrorResponseType);
        Assert.Equal(StaticValues.ErrorUserRegister, exception.Message);
    }

    [Fact(DisplayName = "AuthService: CheckIfUserExists - Should execute successfully if user does not exist.")]
    public async Task CheckIfUserExists_WhenUserDoesNotExist_ShouldPassSuccessfully()
    {
        _userManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync((IdentityUser) null);

        await _authService.CheckIfUserExists(It.IsAny<string>());

        _userManager.Verify(a =>
            a.FindByNameAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact(DisplayName =
        "AuthService: ChangeCustomerPassword - Should successfully change the customer password.")]
    public async Task ChangeCustomerPassword_Success()
    {
        // arrange
        IdentityUser identityUser = _fixture.Create<IdentityUser>();
        Customer mockCustomer = _fixture.Create<Customer>();
        _userManager.Setup(x => x.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(identityUser);
        IdentityResult identityResultMock = IdentityResult.Success;
        ChangeCustomerPasswordRequest changeCustomerPasswordRequestMock =
            _fixture.Create<ChangeCustomerPasswordRequest>();
        _userManager.Setup(x => x.ChangePasswordAsync(identityUser, changeCustomerPasswordRequestMock.CurrentPassword,
                changeCustomerPasswordRequestMock.NewPassword))
            .ReturnsAsync(identityResultMock);
        _customerRepositoryMock.Setup(x => x.UpdateCustomer(It.IsAny<Customer>())).ReturnsAsync(mockCustomer);
        _customerRepositoryMock.Setup(x => x.GetCustomerByEmail(It.IsAny<string>())).ReturnsAsync(mockCustomer);

        //act
        await _authService.ChangeCustomerPassword(changeCustomerPasswordRequestMock, "customer@mail.com");

        // assert
        _userManager.Verify(x => x.FindByNameAsync(It.IsAny<string>()), Times.Once);
        _userManager.Verify(x => x.ChangePasswordAsync(identityUser, changeCustomerPasswordRequestMock.CurrentPassword,
            changeCustomerPasswordRequestMock.NewPassword), Times.Once);
        Assert.Equal(mockCustomer.PasswordResetDate, DateTime.UtcNow.Date);
    }

    [Fact(DisplayName =
        "AuthService: ChangeCustomerPassword - Should throw business rule violation exception if the change password fails.")]
    public async Task ChangeCustomerPassword_ChangedPasswordFailed_ThrowsBusinessRuleViolationException()
    {
        // arrange
        IdentityUser identityUser = _fixture.Create<IdentityUser>();
        _userManager.Setup(x => x.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(identityUser);
        IdentityResult identityResultMock = IdentityResult.Failed(
            new IdentityError
            {
                Code = "PasswordMismatch",
                Description = "Incorrect password."
            }
        );

        ChangeCustomerPasswordRequest changeCustomerPasswordRequestMock =
            _fixture.Create<ChangeCustomerPasswordRequest>();
        _userManager.Setup(x => x.ChangePasswordAsync(identityUser, changeCustomerPasswordRequestMock.CurrentPassword,
                changeCustomerPasswordRequestMock.NewPassword))
            .ReturnsAsync(identityResultMock);


        BusinessRuleViolationException exception =
            await Assert.ThrowsAsync<BusinessRuleViolationException>(async () =>
                await _authService.ChangeCustomerPassword(changeCustomerPasswordRequestMock, "customer@mail.com"));

        // assert
        _userManager.Verify(x => x.FindByNameAsync(It.IsAny<string>()), Times.Once);
        _userManager.Verify(x => x.ChangePasswordAsync(identityUser, changeCustomerPasswordRequestMock.CurrentPassword,
            changeCustomerPasswordRequestMock.NewPassword), Times.Once);
        Assert.Equal(StaticValues.ChangedPasswordFailed, exception.ErrorResponseType);
        Assert.Equal(StaticValues.ErrorChangePassword, exception.Message);
        Assert.Equal(identityResultMock.Errors, exception.Errors);
    }

    [Fact(DisplayName = "AuthService: LogoutCustomer - Should remove refresh token and logout customer.")]
    public async Task LogoutCustomer_Success()
    {
        // arrange
        RefreshTokenRequest request = _fixture.Build<RefreshTokenRequest>()
            .With(x => x.AccessToken, mockJwtToken)
            .With(x => x.RefreshToken, "refresh_token")
            .Create();
        Account account = _fixture.Build<Account>()
            .With(x => x.Id)
            .With(x => x.Email)
            .Create();

        _accountServiceMock.Setup(u => u
                .GetAccount(It.IsAny<string>()))
            .ReturnsAsync(account);
        _cache.Setup(c => c
                .GetAsync(It.IsAny<string>(), default))
            .ReturnsAsync(Encoding.ASCII.GetBytes("refresh_token"));

        // act
        await _authService.LogoutCustomer(request);

        // assert
        _cache.Verify(x => x.RemoveAsync($"{account.Id}_{StaticValues.RefreshTokenKey}", default), Times.Once);
        _signInManager.Verify(x => x.SignOutAsync(), Times.Once);
    }
}
