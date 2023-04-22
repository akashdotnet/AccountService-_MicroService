using System.ComponentModel.DataAnnotations;

namespace AccountService.API.Dto.Request;

public class CustomerSetNewPassword
{
    [Required] public string Token { get; set; }

    [Required] public string Password { get; set; }
}
