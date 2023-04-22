using System.Text.Json.Serialization;

namespace AccountService.API.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExpertStepEnum
{
    SignUpComplete,
    ExpertProfileCompletion
}
