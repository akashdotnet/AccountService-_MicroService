using System.ComponentModel.DataAnnotations;

namespace AccountService.API.Dto.Request;

public class CustomerSuggestedDealerRequest
{
    [Required] public string DealerName { get; set; }
    [Required] public string DealerAddress { get; set; }
    [Required] [EmailAddress] public string DealerEmail { get; set; }
}
