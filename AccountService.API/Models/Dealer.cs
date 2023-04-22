using System.Collections.Generic;
using AccountService.API.Enums;
using Microsoft.EntityFrameworkCore;

namespace AccountService.API.Models;

[Index("AccountId", IsUnique = true)]
public class Dealer : BaseModel
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public Account Account { get; set; }
    public DealerStepEnum LastCompletedOnboardingStep { get; set; }
    public int? BusinessId { get; set; }
    public Business? Business { get; set; }
    public List<string>? Certifications { get; set; }
    public bool TermsAndConditionsAccepted { get; set; }
    public bool ReceivePromotionalContent { get; set; }
}
