using System.Threading.Tasks;
using AccountService.API.Models;

namespace AccountService.API.Repositories.Interfaces;

public interface IAccountRepository
{
    Task<Account> CreateAccount(Account account);
    Task<Account?> GetAccountByEmail(string email);
    Task<bool> GetFreeCallStatusByEmail(string email);
}
