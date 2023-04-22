using System.Threading.Tasks;
using AccountService.API.Data;
using AccountService.API.Models;
using AccountService.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AccountService.API.Repositories;

public class AddressRepository : IAddressRepository
{
    private readonly AccountServiceDbContext _accountServiceDbContext;

    public AddressRepository(AccountServiceDbContext accountServiceDbContext)
    {
        _accountServiceDbContext = accountServiceDbContext;
    }

    public async Task<Address> CreateOrUpdateAddress(Address address)
    {
        EntityEntry<Address> entityEntry = address.Id > 0
            ? _accountServiceDbContext.Address.Update(address)
            : await _accountServiceDbContext.Address.AddAsync(address);
        await _accountServiceDbContext.SaveChangesAsync();
        return entityEntry.Entity;
    }
}
