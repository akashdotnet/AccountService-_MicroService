using System.Collections.Generic;

namespace AccountService.API.Dto.CatalogServiceClient;

public class CountyResponse
{
    public string Name { get; set; }
    public List<string> ZipCodes { get; set; }
}
