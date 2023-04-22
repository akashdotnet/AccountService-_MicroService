using System.Collections.Generic;
using AccountService.API.Dto.BaseDto;
using AccountService.API.Enums;

namespace AccountService.API.Dto.Response;

public class DealerResponse : BaseDealerTermsAndConditions
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public BusinessResponse? Business { get; set; }
    public bool IsOnboardingComplete { get; set; }
    public List<string>? Certifications { get; set; }
    public DealerStepEnum LastCompletedOnboardingStep { get; set; }
}
