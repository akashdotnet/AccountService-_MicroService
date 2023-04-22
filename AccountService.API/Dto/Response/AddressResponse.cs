namespace AccountService.API.Dto.Response;

public class AddressResponse
{
    public string AddressValue { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
    public bool IsPrimaryAddress { get; set; }
}
