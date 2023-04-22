namespace AccountService.API.Dto.PaymentServiceClient;

public class CreateCustomerForPaymentRequest
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
}
