namespace AccountService.API.Models;

public class Address : BaseModel
{
    public int Id { get; set; }
    public string? AddressValue { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public bool IsPrimaryAddress { get; set; }
}
