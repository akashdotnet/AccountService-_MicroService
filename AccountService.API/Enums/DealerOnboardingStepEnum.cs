using System.Text.Json.Serialization;

namespace AccountService.API.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DealerOnboardingStepEnum
{
    AboutBusiness = 0,
    PublicCompanyProfile = 1
}
