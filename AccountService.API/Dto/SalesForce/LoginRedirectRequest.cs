using System.ComponentModel.DataAnnotations;

namespace AccountService.API.Dto.SalesForce;

public class LoginRedirectRequest
{
    [Required] public string CallbackUrl { get; set; }
}
