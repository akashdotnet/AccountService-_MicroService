using System.ComponentModel.DataAnnotations;

namespace AccountService.API.Dto.Request;

public class DealerRegistrationRequest : BaseUserRegistrationRequest
{
    [Phone] public string PhoneNumber { get; set; }
}
