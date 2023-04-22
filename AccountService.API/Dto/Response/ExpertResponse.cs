using System.Collections.Generic;
using AccountService.API.Enums;

namespace AccountService.API.Dto.Response;

public class ExpertResponse
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public bool IsOnboardingComplete { get; set; }
    public string ZipCode { get; set; }
    public string ExperienceInYears { get; set; }
    public List<LanguageResponse> Languages { get; set; }
    public List<SkillResponse> Skills { get; set; }
    public string ProfilePhotoUrl { get; set; }
    public string About { get; set; }
    public ExpertStepEnum LastCompletedOnboardingStep { get; set; }
}
