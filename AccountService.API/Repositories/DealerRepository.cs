using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountService.API.Data;
using AccountService.API.Models;
using AccountService.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AccountService.API.Repositories;

public class DealerRepository : IDealerRepository
{
    private readonly AccountServiceDbContext _accountServiceDbContext;

    public DealerRepository(AccountServiceDbContext accountServiceDbContext)
    {
        _accountServiceDbContext = accountServiceDbContext;
    }

    public async Task<Dealer> CreateDealer(Dealer dealer)
    {
        await _accountServiceDbContext.Dealers.AddAsync(dealer);
        await _accountServiceDbContext.SaveChangesAsync();
        return dealer;
    }

    public async Task<Dealer> UpdateDealer(Dealer dealer)
    {
        EntityEntry<Dealer> savedMovieEntity = _accountServiceDbContext.Entry(dealer);
        _accountServiceDbContext.Entry(dealer).State = EntityState.Modified;
        await _accountServiceDbContext.SaveChangesAsync();
        return savedMovieEntity.Entity;
    }

    public async Task<Dealer?> GetDealerByAccountId(int accountId)
    {
        return await _accountServiceDbContext.Dealers
            .Include("Account")
            .Include("Business")
            .Include("Business.Brands")
            .Include("Business.Locations")
            .Include("Business.Locations.BusinessLocationServiceableCounties")
            .Include("Business.Locations.Address")
            .Include("Business.Categories")
            .SingleOrDefaultAsync(d => d.Account.Id == accountId);
    }

    public async Task<Dealer?> GetDealerAndBusinessByAccountId(int accountId)
    {
        return await _accountServiceDbContext.Dealers
            .Include("Account")
            .Include("Business")
            .SingleOrDefaultAsync(d => d.Account.Id == accountId);
    }

    public async Task<List<Dealer>> GetDealerByBusinessLocationIds(List<int> businessLocationIds,
        bool? isOnboardingComplete = null)
    {
        return await _accountServiceDbContext.Dealers
            .Include("Account")
            .Include("Business")
            // filter the business location based on businessLocationIds
            .Include(dealer => dealer.Business.Locations.Where(businessLocation =>
                businessLocationIds.Contains(businessLocation.Id)))
            .Include("Business.Locations.BusinessLocationServiceableCounties")
            .Include("Business.Locations.Address")
            .Include("Business.Categories")
            // filter the dealer based on at least one of the location has same id as businessLocationIds
            .Where(dealer => dealer.Business != null
                             && dealer.Business.Locations.Any(businessLocation =>
                                 businessLocationIds.Contains(businessLocation.Id))
                             && (isOnboardingComplete == null ||
                                 dealer.Account.IsOnboardingComplete == isOnboardingComplete)
            )
            .ToListAsync();
    }

    public async Task<List<Dealer>> GetDealersBySearchStringAndState(string businessSearchString, string state)
    {
        return await _accountServiceDbContext.Dealers
            .Include("Business")
            .Include("Business.Locations")
            .Include("Business.Locations.Address")
            .Where(d => d.Business != null && d.Business.Name.ToUpper().StartsWith(businessSearchString.ToUpper()))
            .Where(d => d.Business.Locations.Any(l => l.Address.State.Equals(state)))
            // defining the explicit mapping here so that only selected columns can be fetched
            .Select(d => new Dealer
            {
                Account = new Account
                {
                    Email = d.Account.Email
                },
                Business = new Business
                {
                    Name = d.Business!.Name,
                    Locations = d.Business.Locations
                }
            })
            .AsNoTracking()
            .ToListAsync();
    }
}
