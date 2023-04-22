using System.ComponentModel.DataAnnotations;

namespace AccountService.API.Dto.SalesForce;

public class TokenRequest
{
    [Required] public string AuthCode { get; set; }
    public string? CallbackUrl { get; set; }
}
