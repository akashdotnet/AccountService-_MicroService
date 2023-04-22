using System.ComponentModel.DataAnnotations;
using AccountService.API.Enums;
using AccountService.API.Validations;

namespace AccountService.API.Dto.Request;

public class ExpertRegistrationRequest : BaseUserRegistrationRequest
{
    [Required]
    [Email(EmailTypeEnum.Pentair)]
    public override string Email { get; set; }

    [Phone] public string PhoneNumber { get; set; }
}
