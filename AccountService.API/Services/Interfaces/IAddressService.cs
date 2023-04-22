using System.Threading.Tasks;
using AccountService.API.Dto.Request;

namespace AccountService.API.Services.Interfaces;

public interface IAddressService
{
    Task ValidateAddress(CustomerAddressRequest customerAddressRequest);
}
