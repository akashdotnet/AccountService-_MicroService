using System.Threading.Tasks;
using AccountService.API.Models;

namespace AccountService.API.Services.Interfaces;

public interface IAccountService
{
    Task<Account?> GetAccount(string email);
    Task<int> GetAccountId(string? email = null);
}
