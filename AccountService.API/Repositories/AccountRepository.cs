using System.Threading.Tasks;
using AccountService.API.Data;
using AccountService.API.Models;
using AccountService.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AccountService.API.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly AccountServiceDbContext _accountServiceDbContext;

    public AccountRepository(AccountServiceDbContext dbContext)
    {
        _accountServiceDbContext = dbContext;
    }

    public async Task<Account> CreateAccount(Account account)
    {
        await _accountServiceDbContext.Accounts.AddAsync(account);
        await _accountServiceDbContext.SaveChangesAsync();
        return account;
    }

    public async Task<Account?> GetAccountByEmail(string email)
    {
        return await _accountServiceDbContext.Accounts.SingleOrDefaultAsync(account => account.Email.Equals(email));
    }

    public async Task<bool> GetFreeCallStatusByEmail(string email)
    {
        return (await _accountServiceDbContext.Customers
            .Include(customer => customer.Account)
            .SingleOrDefaultAsync(customer => customer.Account.Email.Equals(email)))?.FirstFreeCallAvailed ?? false;
    }
}
