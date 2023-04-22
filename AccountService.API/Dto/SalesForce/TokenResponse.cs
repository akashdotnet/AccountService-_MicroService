using Newtonsoft.Json;

namespace AccountService.API.Dto.SalesForce;

public class TokenResponse
{
    [JsonProperty(PropertyName = "access_token")]
    public string AccessToken { get; set; }

    [JsonProperty(PropertyName = "id_token")]
    public string IdToken { get; set; }

    [JsonProperty(PropertyName = "refresh_token")]
    public string RefreshToken { get; set; }

    [JsonProperty(PropertyName = "token_type")]
    public string TokenType { get; set; }
}
