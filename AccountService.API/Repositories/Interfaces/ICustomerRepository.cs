using System.Collections.Generic;
using System.Threading.Tasks;
using AccountService.API.Models;

namespace AccountService.API.Repositories.Interfaces;

public interface ICustomerRepository
{
    Task<Customer> CreateCustomer(Customer customer);
    Task<Customer?> GetCustomerByAccountId(int accountId);
    Task<List<Customer>> GetCustomersByEmailIds(List<string> emailIds);
    Task<Customer> UpdateCustomer(Customer customer);
    Task DeleteCustomerAccount(Customer customer);
    Task<int?> GetCustomerIdByEmail(string email);
    Task<Customer?> GetCustomerByEmail(string email);
}
