using System.Collections.Generic;

namespace AccountService.API.Dto.Response;

public class BusinessResponse
{
    public string Name { get; set; }
    public List<BusinessLocationResponse> Locations { get; set; }
    public string? About { get; set; }
    public string? WebsiteUrl { get; set; }
    public int? PoolCount { get; set; }
    public string? StartYear { get; set; }
    public string? ExperienceInYears { get; set; }
    public string? PhoneNumber { get; set; }
    public string? PreferredCommunicationsEmail { get; set; }
    public string? LogoUrl { get; set; }
    public int? LogoBlobId { get; set; }
    public List<BusinessBrandResponse> Brands { get; set; }

    public List<JobCategoryResponse> Categories { get; set; }
}
