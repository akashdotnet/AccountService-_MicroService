namespace AccountService.API.Dto.Request;

public class CustomerRegistrationRequest : BaseUserRegistrationRequest
{
    public bool ReceivePromotionalContent { get; set; }
    public bool TermsAndConditionsAccepted { get; set; }
}
