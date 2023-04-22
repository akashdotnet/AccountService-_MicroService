namespace AccountService.API.Models;

public class CustomerSuggestedDealer : BaseModel
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; }
    public string DealerName { get; set; }
    public string DealerAddress { get; set; }
    public string DealerEmail { get; set; }
}
