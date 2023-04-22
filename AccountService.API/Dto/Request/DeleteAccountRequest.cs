namespace AccountService.API.Dto.Request;

public class DeleteAccountRequest
{
    public string CustomerEmail { get; set; }
    public int CustomerId { get; set; }
}
