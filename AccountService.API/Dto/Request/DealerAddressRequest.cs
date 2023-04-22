namespace AccountService.API.Dto.Request;

public class DealerAddressRequest : BaseAddressRequest
{
    public bool IsPrimaryAddress { get; set; }
}
