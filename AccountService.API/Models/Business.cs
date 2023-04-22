using System.Collections.Generic;

namespace AccountService.API.Models;

public class Business : BaseModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int? LogoBlobId { get; set; }
    public string? About { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public string? PreferredCommunicationsEmail { get; set; }
    public int? StartYear { get; set; }
    public int? PoolCount { get; set; }
    public List<BusinessLocation> Locations { get; set; } = new();
    public List<BusinessBrand> Brands { get; set; } = new();
    public List<BusinessJobCategory> Categories { get; set; } = new();
}
