using System.Collections.Generic;
using System.Threading.Tasks;
using AccountService.API.Models;

namespace AccountService.API.Repositories.Interfaces;

public interface IDealerRepository
{
    Task<Dealer> CreateDealer(Dealer dealer);
    Task<Dealer> UpdateDealer(Dealer dealer);
    Task<Dealer?> GetDealerByAccountId(int accountId);
    Task<Dealer?> GetDealerAndBusinessByAccountId(int accountId);
    Task<List<Dealer>> GetDealerByBusinessLocationIds(List<int> businessLocationIds, bool? isOnboardingComplete = null);

    Task<List<Dealer>> GetDealersBySearchStringAndState(string businessSearchString, string state);
}
