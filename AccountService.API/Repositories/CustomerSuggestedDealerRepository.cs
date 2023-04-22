using System.Threading.Tasks;
using AccountService.API.Data;
using AccountService.API.Models;
using AccountService.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AccountService.API.Repositories;

public class CustomerSuggestedDealerRepository : ICustomerSuggestedDealerRepository
{
    private readonly AccountServiceDbContext _accountServiceDbContext;

    public CustomerSuggestedDealerRepository(AccountServiceDbContext accountServiceDbContext)
    {
        _accountServiceDbContext = accountServiceDbContext;
    }

    public async Task<CustomerSuggestedDealer> CreateCustomerSuggestedDealer(
        CustomerSuggestedDealer customerSuggestedDealer)
    {
        await _accountServiceDbContext.CustomerSuggestedDealers.AddAsync(customerSuggestedDealer);
        await _accountServiceDbContext.SaveChangesAsync();
        return customerSuggestedDealer;
    }

    public async Task<CustomerSuggestedDealer> UpdateCustomerSuggestedDealer(
        CustomerSuggestedDealer customerSuggestedDealer)
    {
        EntityEntry<CustomerSuggestedDealer> savedCustomerEntity =
            _accountServiceDbContext.Entry(customerSuggestedDealer);
        _accountServiceDbContext.Entry(customerSuggestedDealer).State = EntityState.Modified;
        await _accountServiceDbContext.SaveChangesAsync();
        return savedCustomerEntity.Entity;
    }

    public async Task<CustomerSuggestedDealer?> GetCustomerSuggestedDealer(string dealerEmail, string customerEmail)
    {
        return await _accountServiceDbContext.CustomerSuggestedDealers
            .Include("Customer")
            .SingleOrDefaultAsync(suggestedDealer => dealerEmail.Equals(suggestedDealer.DealerEmail)
                                                     && suggestedDealer.Customer.Account.Email.Equals(customerEmail));
    }
}
