using System.ComponentModel.DataAnnotations;

namespace AccountService.API.Dto.Request;

public class LoginRequest
{
    [Required] [EmailAddress] public string Email { get; set; }

    [Required] public string Password { get; set; }
}
