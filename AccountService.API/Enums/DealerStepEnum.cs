using System.Text.Json.Serialization;

namespace AccountService.API.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DealerStepEnum
{
    SignUpComplete = 0,
    AboutBusiness = 1,
    PublicCompanyProfile = 2
}
