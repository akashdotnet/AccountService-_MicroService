using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountService.API.Data;
using AccountService.API.Models;
using AccountService.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AccountService.API.Repositories;

public class ExpertRepository : IExpertRepository
{
    private readonly AccountServiceDbContext _accountServiceDbContext;

    public ExpertRepository(AccountServiceDbContext accountServiceDbContext)
    {
        _accountServiceDbContext = accountServiceDbContext;
    }

    public async Task<Expert> UpdateExpert(Expert expert)
    {
        EntityEntry<Expert> savedExpertEntity = _accountServiceDbContext.Entry(expert);
        _accountServiceDbContext.Entry(expert).State = EntityState.Modified;
        await _accountServiceDbContext.SaveChangesAsync();
        return savedExpertEntity.Entity;
    }

    public async Task<Expert> CreateExpert(Expert expert)
    {
        await _accountServiceDbContext.Experts.AddAsync(expert);
        await _accountServiceDbContext.SaveChangesAsync();
        return expert;
    }

    public async Task<Expert?> GetExpertByAccountId(int accountId)
    {
        return await _accountServiceDbContext.Experts
            .Include("Account")
            .Include("Skills")
            .Include("Languages")
            .SingleOrDefaultAsync(e => e.Account.Id == accountId);
    }

    public async Task<List<Expert>> GetExperts()
    {
        return await _accountServiceDbContext.Experts
            .Include("Account")
            .Include("Skills")
            .Include("Languages")
            .Where(expert => expert.Account.IsOnboardingComplete == true)
            .ToListAsync();
    }

    public async Task<List<Expert>> GetExpertsByEmailIds(List<string> emailIds)
    {
        return await _accountServiceDbContext.Experts
            .Include("Account")
            .Where(c => emailIds.Contains(c.Account.Email) && c.Account.IsOnboardingComplete)
            .ToListAsync();
    }
}
