using System.Collections.Generic;

namespace AccountService.API.Dto.Request;

public class ExpertByEmailRequest
{
    public List<string> Emails { get; set; }
}
