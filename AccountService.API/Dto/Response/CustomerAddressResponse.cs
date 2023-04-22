namespace AccountService.API.Dto.Response;

public class CustomerAddressResponse
{
    public string AddressValue { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
    public bool IsPrimaryAddress { get; set; }
}
