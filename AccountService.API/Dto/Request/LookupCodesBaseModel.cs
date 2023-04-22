using System.Collections.Generic;

namespace AccountService.API.Dto.Request;

public class LookupCodesBaseModel
{
    public LookupCodesBaseModel()
    {
        Codes = new List<string>();
    }

    public List<string> Codes { get; set; }
    public string? Others { get; set; }
}
