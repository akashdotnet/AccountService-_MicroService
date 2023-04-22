using System.Threading.Tasks;
using AccountService.API.Data;
using AccountService.API.Models;
using AccountService.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AccountService.API.Repositories;

public class BusinessRepository : IBusinessRepository
{
    private readonly AccountServiceDbContext _accountServiceDbContext;

    public BusinessRepository(AccountServiceDbContext accountServiceDbContext)
    {
        _accountServiceDbContext = accountServiceDbContext;
    }

    public async Task<Business> SaveBusiness(Business business)
    {
        EntityEntry<Business> savedBusinessEntity = _accountServiceDbContext.Entry(business);
        _accountServiceDbContext.Entry(business).State = EntityState.Modified;
        await _accountServiceDbContext.SaveChangesAsync();
        return savedBusinessEntity.Entity;
    }
}
