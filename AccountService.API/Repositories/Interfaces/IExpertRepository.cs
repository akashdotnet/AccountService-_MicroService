using System.Collections.Generic;
using System.Threading.Tasks;
using AccountService.API.Models;

namespace AccountService.API.Repositories.Interfaces;

public interface IExpertRepository
{
    Task<Expert> CreateExpert(Expert expert);
    Task<Expert?> GetExpertByAccountId(int accountId);
    Task<Expert> UpdateExpert(Expert expert);
    Task<List<Expert>> GetExperts();
    Task<List<Expert>> GetExpertsByEmailIds(List<string> emailIds);
}
