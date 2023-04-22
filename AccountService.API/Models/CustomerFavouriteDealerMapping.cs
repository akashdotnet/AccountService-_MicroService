using Microsoft.EntityFrameworkCore;

namespace AccountService.API.Models;

[Index(nameof(CustomerId), IsUnique = true)]
public class CustomerFavouriteDealerMapping : BaseModel
{
    public int Id { get; set; }
    public BusinessLocation BusinessLocation { get; set; }
    public Customer Customer { get; set; }
    public int BusinessLocationId { get; set; }
    public int CustomerId { get; set; }
}
