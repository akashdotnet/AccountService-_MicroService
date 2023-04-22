using System.ComponentModel.DataAnnotations;
using AccountService.API.Constants;
using AccountService.API.Enums;

namespace AccountService.API.Dto.Request;

public class UpdateExpertRequest
{
    [RegularExpression(StaticValues.ZipCodeRegex, ErrorMessage = StaticValues.ErrorInvalidZipCode)]
    public string? ZipCode { get; set; }

    public string? ExperienceInYears { get; set; }

    public LanguageRequest? Languages { get; set; }
    public SkillRequest? Skills { get; set; }

    [MaxLength(StaticValues.AboutMaxLength)]
    public string? About { get; set; }

    [Required] public ExpertOnboardingStepEnum OnboardingStep { get; set; }
}
