using System.Collections.Generic;
using System.Threading.Tasks;
using AccountService.API.Dto.CustomerServiceClient;

namespace AccountService.API.Clients.Interfaces;

public interface ICustomerServiceClient
{
    Task<List<ReminderResponse>> CreateCustomerReminders(CreateCustomerReminderRequest createCustomerReminderRequest);
}
