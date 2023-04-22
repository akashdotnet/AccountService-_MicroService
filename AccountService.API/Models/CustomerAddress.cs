namespace AccountService.API.Models;

public class CustomerAddress : BaseModel
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int AddressId { get; set; }
    public Customer Customer { get; set; }
    public Address Address { get; set; }
}
