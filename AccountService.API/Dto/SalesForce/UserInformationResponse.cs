using Newtonsoft.Json;

namespace AccountService.API.Dto.SalesForce;

public class UserInformationResponse
{
    [JsonProperty(PropertyName = "user_id")]
    public string UserId { get; set; }

    public string Name { get; set; }
    public string Email { get; set; }

    [JsonProperty(PropertyName = "given_name")]
    public string FirstName { get; set; }

    [JsonProperty(PropertyName = "family_name")]
    public string LastName { get; set; }

    [JsonProperty(PropertyName = "email_verified")]
    public string EmailVerified { get; set; }

    [JsonProperty(PropertyName = "custom_attributes")]
    public CustomAttributes CustomAttributes { get; set; }
}
