using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AccountService.API.Config;
using AccountService.API.Constants;
using AccountService.API.Dto;
using AccountService.API.Dto.Request;
using AccountService.API.Dto.Response;
using AccountService.API.Enums;
using AccountService.API.Models;
using AccountService.API.Repositories.Interfaces;
using AccountService.API.Services.Interfaces;
using AccountService.API.Utils.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using PodCommonsLibrary.Core.Enums;
using PodCommonsLibrary.Core.Exceptions;
using Serilog;

namespace AccountService.API.Services;

public class AuthService : IAuthService
{
    private readonly IAccountService _accountService;
    private readonly IDistributedCache _cache;
    private readonly ICustomerRepository _customerRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly TokenConfig _tokenConfig;
    private readonly UserManager<IdentityUser> _userManager;

    public AuthService(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        TokenConfig tokenConfig,
        IDistributedCache cache,
        IAccountService accountService,
        IHttpContextAccessor httpContextAccessor,
        ICustomerRepository customerRepository
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenConfig = tokenConfig;
        _cache = cache;
        _accountService = accountService;
        _httpContextAccessor = httpContextAccessor;
        _customerRepository = customerRepository;
    }

    public async Task<IdentityResult?> CreateIdentityUser(
        BaseUserRegistrationRequest baseUserRegistrationRequest,
        UserRoleEnum userRoleEnum
    )
    {
        // check if user exists
        IdentityUser? userExists = await _userManager.FindByNameAsync(baseUserRegistrationRequest.Email);
        if (userExists != null)
        {
            throw new BusinessRuleViolationException(
                StaticValues.UserWithSameEmailExists,
                StaticValues.ErrorUserRegister
            );
        }

        IdentityUser user = new()
        {
            Email = baseUserRegistrationRequest.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = baseUserRegistrationRequest.Email
        };

        // create identity user - asp_net_users
        IdentityResult? result = await _userManager.CreateAsync(user, baseUserRegistrationRequest.Password);
        if (!result.Succeeded)
        {
            throw new BusinessRuleViolationException(
                StaticValues.RegistrationFailed,
                result.Errors.Aggregate("", (acc, item) => string.Concat(acc, item.Description))
            );
        }

        // assign given role
        await _userManager.AddToRoleAsync(user, userRoleEnum.ToString());

        return result;
    }

    public async Task<LoginResponse> GenerateToken(LoginRequest loginRequest)
    {
        IdentityUser user = await GetIdentityUser(loginRequest.Email);
        Account account = await GetUserAccount(loginRequest.Email);
        SignInResult result = await _signInManager.PasswordSignInAsync(
            loginRequest.Email,
            loginRequest.Password,
            false,
            false
        );
        if (!result.Succeeded)
        {
            throw new UnauthorizedException(StaticValues.ErrorIncorrectCredentials);
        }

        return await GetAuthTokens(user, account);
    }

    public async Task<LoginResponse> RefreshToken(RefreshTokenRequest refreshTokenRequest)
    {
        JwtSecurityTokenHandler handler = new();
        JwtSecurityToken? decodedAccessToken = handler.ReadJwtToken(refreshTokenRequest.AccessToken);
        string email = decodedAccessToken.Claims.First(claim => claim.Type == StaticValues.EmailClaim).Value;
        IdentityUser user = await GetIdentityUser(email);
        Account account = await GetUserAccount(email);
        string cacheRefreshToken = await _cache.GetStringAsync($"{account.Id}_{StaticValues.RefreshTokenKey}");
        if (refreshTokenRequest.RefreshToken != cacheRefreshToken)
        {
            throw new UnauthorizedException(StaticValues.ErrorInvalidRefreshToken);
        }

        return await GetAuthTokens(user, account);
    }

    public async Task<Session> GenerateResetToken(CustomerResetPasswordRequest customerResetPasswordRequest)
    {
        string token = Guid.NewGuid().ToString();
        IdentityUser? user = await GetIdentityUser(customerResetPasswordRequest.Email);
        CustomerSession session = new()
        {
            Email = user.Email,
            OtpVerified = false,
            Type = EmailTemplateEnum.Reset
        };
        await _cache.SetAsync(token, session.ObjectToByteArray());
        return new Session
        {
            SessionId = token
        };
    }

    public async Task<IdentityResult> SetNewPassword(CustomerSetNewPassword customerSetNewPassword)
    {
        byte[] data = await _cache.GetAsync(customerSetNewPassword.Token);
        CustomerSession? session = data?.ByteArrayToObject<CustomerSession>();
        if (session?.Email == null)
        {
            throw new UnauthorizedException(StaticValues.ErrorInvalidRefreshToken);
        }

        if (session.OtpVerified != true)
        {
            throw new UnauthorizedException(StaticValues.ErrorRefreshTokenOTPInvalid);
        }

        IdentityUser user = await GetIdentityUser(session.Email);
        string token = await _userManager.GeneratePasswordResetTokenAsync(user);
        IdentityResult? result = await _userManager.ResetPasswordAsync(user, token, customerSetNewPassword.Password);
        if (!result.Succeeded)
        {
            throw new UnauthorizedException(result.ToString());
        }

        Customer? customer = await _customerRepository.GetCustomerByEmail(session.Email);
        customer.PasswordResetDate = DateTime.UtcNow.Date;
        await _customerRepository.UpdateCustomer(customer);
        _cache.RemoveAsync(customerSetNewPassword.Token);
        return result;
    }

    public async Task ChangeCustomerPassword(
        ChangeCustomerPasswordRequest changeCustomerPasswordRequest,
        string customerEmail
    )
    {
        IdentityUser user = await GetIdentityUser(customerEmail);
        IdentityResult identityResult = await _userManager.ChangePasswordAsync(
            user,
            changeCustomerPasswordRequest.CurrentPassword,
            changeCustomerPasswordRequest.NewPassword
        );
        if (!identityResult.Succeeded)
        {
            throw new BusinessRuleViolationException(
                StaticValues.ChangedPasswordFailed,
                StaticValues.ErrorChangePassword,
                identityResult.Errors
            );
        }

        Customer? customer = await _customerRepository.GetCustomerByEmail(customerEmail);
        customer.PasswordResetDate = DateTime.UtcNow.Date;
        await _customerRepository.UpdateCustomer(customer);
    }

    public async Task CheckIfUserExists(string email)
    {
        // check if user exists
        IdentityUser? userExists = await _userManager.FindByNameAsync(email);
        if (userExists != null)
        {
            throw new BusinessRuleViolationException(
                StaticValues.UserWithSameEmailExists,
                StaticValues.ErrorUserRegister
            );
        }
    }

    public async Task LogoutCustomer(RefreshTokenRequest refreshTokenRequest)
    {
        JwtSecurityTokenHandler handler = new();
        JwtSecurityToken? decodedAccessToken = handler.ReadJwtToken(refreshTokenRequest.AccessToken);
        string email = decodedAccessToken.Claims.First(claim => claim.Type == StaticValues.EmailClaim).Value;
        Account account = await GetUserAccount(email);
        await _cache.RemoveAsync($"{account.Id}_{StaticValues.RefreshTokenKey}");

        await _signInManager.SignOutAsync();
        Log.Information("User with account Id - {AccountId} logged out.", account.Id);
    }

    private async Task<IdentityUser> GetIdentityUser(string email)
    {
        IdentityUser? user = await _userManager.FindByNameAsync(email);
        if (user == null)
        {
            throw new BusinessRuleViolationException(
                StaticValues.UserDoesNotExist,
                StaticValues.ErrorResetToken
            );
        }

        return user;
    }

    private async Task<Account> GetUserAccount(string email)
    {
        Account? account = await _accountService.GetAccount(email);
        if (account == null)
        {
            throw new BusinessRuleViolationException(
                StaticValues.UserDoesNotExist,
                StaticValues.ErrorUserLogin
            );
        }

        return account;
    }

    private async Task<LoginResponse> GetAuthTokens(IdentityUser user, Account account)
    {
        IList<string>? userRoles = await _userManager.GetRolesAsync(user);
        ClaimsIdentity claimsIdentity = new(new[]
        {
            new Claim(StaticValues.EmailClaim, user.UserName)
        });
        foreach (string userRole in userRoles)
        {
            claimsIdentity.AddClaim(new Claim(StaticValues.UserTypeClaim, userRole));
        }

        string token = _tokenConfig.GetJwtToken(claimsIdentity);
        string refreshToken = _tokenConfig.GetRefreshToken();
        LoginResponse loginResponse = new()
        {
            AccessToken = token,
            RefreshToken = refreshToken
        };
        DistributedCacheEntryOptions options = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
        };

        await _cache.SetStringAsync($"{account.Id}_{StaticValues.RefreshTokenKey}", refreshToken, options);
        return loginResponse;
    }
}
