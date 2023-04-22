using System.Threading.Tasks;
using AccountService.API.Models;

namespace AccountService.API.Repositories.Interfaces;

public interface ICustomerSuggestedDealerRepository
{
    Task<CustomerSuggestedDealer> CreateCustomerSuggestedDealer(CustomerSuggestedDealer customerSuggestedDealer);

    Task<CustomerSuggestedDealer>
        UpdateCustomerSuggestedDealer(CustomerSuggestedDealer customerSuggestedDealer);

    Task<CustomerSuggestedDealer?> GetCustomerSuggestedDealer(string dealerEmail, string customerEmail);
}
