namespace AccountService.API.Dto.CatalogServiceClient;

public class StateByZipCodeResponse
{
    public string Name { get; set; }
    public string Abbreviation { get; set; }
    public CountyByZipCodeResponse County { get; set; }
}
