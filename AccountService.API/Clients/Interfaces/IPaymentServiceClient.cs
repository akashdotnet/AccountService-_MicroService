using System.Threading.Tasks;
using AccountService.API.Dto.PaymentServiceClient;

namespace AccountService.API.Clients.Interfaces;

public interface IPaymentServiceClient
{
    Task<CreateCustomerForPaymentResponse> CreateCustomer(CreateCustomerForPaymentRequest createCustomerRequest);
    Task CreateDealerBankAccount(string email, string userType);
}
