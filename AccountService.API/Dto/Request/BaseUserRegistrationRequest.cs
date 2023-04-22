using System.ComponentModel.DataAnnotations;
using AccountService.API.Enums;
using AccountService.API.Validations;

namespace AccountService.API.Dto.Request;

public class BaseUserRegistrationRequest
{
    [Required]
    [Email(EmailTypeEnum.NonPentair)]
    public virtual string Email { get; set; }

    [Required] public string Password { get; set; }
    [Required] public string FirstName { get; set; }
    public string? LastName { get; set; }
}
