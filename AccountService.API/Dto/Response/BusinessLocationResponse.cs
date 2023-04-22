using System.Collections.Generic;

namespace AccountService.API.Dto.Response;

public class BusinessLocationResponse : BaseBusinessLocationResponse
{
    public List<string>? ServiceableCounties { get; set; }
}
