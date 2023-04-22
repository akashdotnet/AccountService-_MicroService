using System;

namespace AccountService.API.Dto.CustomerServiceClient;

public class CreateCustomerReminderRequest
{
    public string Email { get; set; }
    public DateTime StartDate { get; set; }
}
