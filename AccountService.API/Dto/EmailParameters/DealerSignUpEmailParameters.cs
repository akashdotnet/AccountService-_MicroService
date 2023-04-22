using AccountService.API.Models;

namespace AccountService.API.Dto.EmailParameters;

public class DealerSignUpEmailParameters
{
    public Address Address;
    public string BusinessName;
    public string Email;
    public string RegistrationDate;
}
