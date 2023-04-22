namespace AccountService.API.Models;

public class BusinessBrand : BaseModel
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public Business Business { get; set; }
    public string Code { get; set; }
    public string? Others { get; set; }
}
