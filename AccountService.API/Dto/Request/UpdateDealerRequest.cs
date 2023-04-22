using System.Collections.Generic;
using AccountService.API.Enums;

namespace AccountService.API.Dto.Request;

public class UpdateDealerRequest
{
    public BusinessRequest Business { get; set; }
    public List<string>? Certifications { get; set; }
    public DealerOnboardingStepEnum OnboardingStep { get; set; }
}
