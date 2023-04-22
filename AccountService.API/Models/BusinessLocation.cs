using System.Collections.Generic;

namespace AccountService.API.Models;

public class BusinessLocation : BaseModel
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public Business Business { get; set; }
    public int AddressId { get; set; }
    public Address Address { get; set; }
    public string OfficeName { get; set; }
    public List<BusinessLocationServiceableCounty> BusinessLocationServiceableCounties { get; set; }
}
