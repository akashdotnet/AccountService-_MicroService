using System.Threading.Tasks;
using AccountService.API.Models;

namespace AccountService.API.Repositories.Interfaces;

public interface IAddressRepository
{
    Task<Address> CreateOrUpdateAddress(Address address);
}
