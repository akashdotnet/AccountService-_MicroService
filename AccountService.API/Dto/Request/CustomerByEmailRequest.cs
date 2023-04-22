using System.Collections.Generic;

namespace AccountService.API.Dto.Request;

public class CustomerByEmailRequest
{
    public List<string> Emails { get; set; }
}
