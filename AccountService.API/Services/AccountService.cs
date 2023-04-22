using System.Threading.Tasks;
using AccountService.API.Constants;
using AccountService.API.Models;
using AccountService.API.Repositories.Interfaces;
using AccountService.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using PodCommonsLibrary.Core.Exceptions;

namespace AccountService.API.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AccountService(IAccountRepository accountRepository, IHttpContextAccessor httpContextAccessor)
    {
        _accountRepository = accountRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Account?> GetAccount(string email)
    {
        return await _accountRepository.GetAccountByEmail(email);
    }

    public async Task<int> GetAccountId(string? email = null)
    {
        string? accountEmail = email ?? _httpContextAccessor.HttpContext?.Request.Headers["x-user-id"];
        if (string.IsNullOrEmpty(accountEmail))
        {
            throw new UnauthorizedException();
        }

        Account? account = await GetAccount(accountEmail);
        if (account == null)
        {
            throw new BusinessRuleViolationException(
                StaticValues.UserDoesNotExist,
                StaticValues.ErrorUserLogin
            );
        }

        return account.Id;
    }
}
