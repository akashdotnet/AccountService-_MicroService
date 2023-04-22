using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountService.API.Data;
using AccountService.API.Models;
using AccountService.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AccountService.API.Repositories;

public class CustomerFavouriteDealerMappingRepository : ICustomerFavouriteDealerMappingRepository
{
    private readonly AccountServiceDbContext _accountServiceDbContext;

    public CustomerFavouriteDealerMappingRepository(AccountServiceDbContext dbContext)
    {
        _accountServiceDbContext = dbContext;
    }

    public async Task<CustomerFavouriteDealerMapping> CreateOrUpdateCustomerFavouriteDealerMapping(
        CustomerFavouriteDealerMapping customerFavouriteDealerMapping)
    {
        EntityEntry<CustomerFavouriteDealerMapping> entityEntry = customerFavouriteDealerMapping.Id > 0
            ? _accountServiceDbContext.CustomerFavouriteDealerMappings.Update(customerFavouriteDealerMapping)
            : await _accountServiceDbContext.CustomerFavouriteDealerMappings.AddAsync(customerFavouriteDealerMapping);
        await _accountServiceDbContext.SaveChangesAsync();
        return entityEntry.Entity;
    }

    public async Task<CustomerFavouriteDealerMapping?> GetCustomerFavouriteDealerMapping(
        int customerId)
    {
        return await _accountServiceDbContext.CustomerFavouriteDealerMappings.SingleOrDefaultAsync(c =>
            c.CustomerId.Equals(customerId));
    }

    public async Task<List<CustomerFavouriteDealerMapping>> GetCustomerFavouriteDealerMapping(List<int> customerIds)
    {
        return await _accountServiceDbContext.CustomerFavouriteDealerMappings.Where(c =>
            customerIds.Contains(c.CustomerId)).ToListAsync();
    }
}
