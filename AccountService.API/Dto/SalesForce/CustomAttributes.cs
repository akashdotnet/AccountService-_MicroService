using Newtonsoft.Json;

namespace AccountService.API.Dto.SalesForce;

public class CustomAttributes
{
    public string Profile { get; set; }
    public string Role { get; set; }

    [JsonProperty(PropertyName = "usertype")]
    public string UserType { get; set; }
}
