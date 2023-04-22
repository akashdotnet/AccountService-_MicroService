using System.ComponentModel.DataAnnotations;

namespace AccountService.API.Dto.Request;

public class CustomerResetPasswordRequest
{
    [Required] [EmailAddress] public string Email { get; set; }
}
