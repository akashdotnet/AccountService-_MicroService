namespace AccountService.API.Models;

public class BusinessLocationServiceableCounty : BaseModel
{
    public int Id { get; set; }
    public int BusinessLocationId { get; set; }
    public BusinessLocation BusinessLocation { get; set; }
    public string ServiceableCounty { get; set; }
}
