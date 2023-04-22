using System.Threading.Tasks;
using AccountService.API.Models;

namespace AccountService.API.Repositories.Interfaces;

public interface ICustomerDeliveryInstructionRepository
{
    Task<CustomerDeliveryInstruction?> CreateDeliveryInstruction(
        CustomerDeliveryInstruction? customerDeliveryInstruction);

    Task<CustomerDeliveryInstruction>
        UpdateDeliveryInstruction(CustomerDeliveryInstruction customerDeliveryInstruction);

    Task<CustomerDeliveryInstruction?> GetDeliveryInstruction(string email);
}
