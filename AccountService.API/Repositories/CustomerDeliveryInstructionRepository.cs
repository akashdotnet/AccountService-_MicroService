using System.Threading.Tasks;
using AccountService.API.Data;
using AccountService.API.Models;
using AccountService.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AccountService.API.Repositories;

public class CustomerDeliveryInstructionRepository : ICustomerDeliveryInstructionRepository
{
    private readonly AccountServiceDbContext _accountServiceDbContext;

    public CustomerDeliveryInstructionRepository(AccountServiceDbContext dbContext)
    {
        _accountServiceDbContext = dbContext;
    }

    public async Task<CustomerDeliveryInstruction?> CreateDeliveryInstruction(
        CustomerDeliveryInstruction? customerDeliveryInstruction)
    {
        await _accountServiceDbContext.CustomerDeliveryInstructions.AddAsync(customerDeliveryInstruction);
        await _accountServiceDbContext.SaveChangesAsync();
        return customerDeliveryInstruction;
    }

    public async Task<CustomerDeliveryInstruction> UpdateDeliveryInstruction(
        CustomerDeliveryInstruction customerDeliveryInstruction)
    {
        EntityEntry<CustomerDeliveryInstruction> savedCustomerEntity =
            _accountServiceDbContext.Entry(customerDeliveryInstruction);
        _accountServiceDbContext.Entry(customerDeliveryInstruction).State = EntityState.Modified;
        await _accountServiceDbContext.SaveChangesAsync();
        return savedCustomerEntity.Entity;
    }

    public async Task<CustomerDeliveryInstruction?> GetDeliveryInstruction(string email)
    {
        return await _accountServiceDbContext.CustomerDeliveryInstructions
            .Include("Customer")
            .SingleOrDefaultAsync(deliveryInstruction =>
                deliveryInstruction != null && email.Equals(deliveryInstruction.Customer.Account.Email));
    }
}
