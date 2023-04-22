using System.Collections.Generic;
using System.Threading.Tasks;
using AccountService.API.Models;

namespace AccountService.API.Repositories.Interfaces;

public interface ICustomerFavouriteDealerMappingRepository
{
    Task<CustomerFavouriteDealerMapping> CreateOrUpdateCustomerFavouriteDealerMapping(
        CustomerFavouriteDealerMapping customerFavouriteDealerMapping);

    Task<CustomerFavouriteDealerMapping?> GetCustomerFavouriteDealerMapping(int customerId);
    Task<List<CustomerFavouriteDealerMapping>> GetCustomerFavouriteDealerMapping(List<int> customerIds);
}
