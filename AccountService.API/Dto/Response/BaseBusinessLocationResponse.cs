namespace AccountService.API.Dto.Response;

public class BaseBusinessLocationResponse
{
    public int Id { get; set; }
    public string OfficeName { get; set; }
    public AddressResponse Address { get; set; }
}
