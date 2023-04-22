using System.Collections.Generic;

namespace AccountService.API.Dto.CatalogServiceClient;

public class StateResponse
{
    public string Name { get; set; }
    public string Abbreviation { get; set; }
    public int DisplayOrder { get; set; }
    public List<CountyResponse> Counties { get; set; }
}
