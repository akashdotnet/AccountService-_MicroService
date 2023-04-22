using System.Collections.Generic;

namespace AccountService.API.Dto.Response;

public class BusinessProfileResponse
{
    public string Name { get; set; }
    public List<BaseBusinessLocationResponse> Locations { get; set; }
}
