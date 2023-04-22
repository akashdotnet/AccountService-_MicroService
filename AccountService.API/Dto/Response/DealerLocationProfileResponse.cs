using System.Collections.Generic;

namespace AccountService.API.Dto.Response;

public class DealerLocationProfileResponse
{
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public BusinessProfileDetailsResponse Business { get; set; }
    public List<string>? Certifications { get; set; }
}
