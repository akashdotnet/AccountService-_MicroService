using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountService.API.Data;
using AccountService.API.Models;
using AccountService.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AccountService.API.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly AccountServiceDbContext _accountServiceDbContext;

    public CustomerRepository(AccountServiceDbContext dbContext)
    {
        _accountServiceDbContext = dbContext;
    }

    public async Task<Customer> CreateCustomer(Customer customer)
    {
        await _accountServiceDbContext.Customers.AddAsync(customer);
        await _accountServiceDbContext.SaveChangesAsync();
        return customer;
    }

    public async Task<Customer?> GetCustomerByAccountId(int accountId)
    {
        return await _accountServiceDbContext.Customers
            .Include("Account")
            .Include("CustomerAddresses.Address")
            .SingleOrDefaultAsync(c => c.Account.Id == accountId);
    }

    public async Task<Customer> UpdateCustomer(Customer customer)
    {
        EntityEntry<Customer> savedCustomerEntity = _accountServiceDbContext.Entry(customer);
        _accountServiceDbContext.Entry(customer).State = EntityState.Modified;
        await _accountServiceDbContext.SaveChangesAsync();
        return savedCustomerEntity.Entity;
    }

    public async Task<List<Customer>> GetCustomersByEmailIds(List<string> emailIds)
    {
        return await _accountServiceDbContext.Customers
            .Include("Account")
            .Include("CustomerAddresses.Address")
            .Where(c => emailIds.Contains(c.Account.Email) && c.Account.IsOnboardingComplete)
            .ToListAsync();
    }

    public async Task DeleteCustomerAccount(Customer customer)
    {
        await _accountServiceDbContext.Database.ExecuteSqlRawAsync(
            $"CALL delete_customer('{customer.Account.Email}','{customer.Id}')");
    }

    public async Task<int?> GetCustomerIdByEmail(string email)
    {
        return await _accountServiceDbContext.Customers
            .Where(customer => customer.Account.Email.Equals(email))
            .Select(customer => customer.Id)
            .SingleOrDefaultAsync();
    }

    public async Task<Customer?> GetCustomerByEmail(string email)
    {
        return await _accountServiceDbContext.Customers
            .Include("Account")
            .Include("CustomerAddresses.Address")
            .SingleOrDefaultAsync(c => c.Account.Email.Equals(email));
    }
}
