using System.Collections.Generic;

namespace AccountService.API.Dto.Response;

public class BusinessProfileDetailsResponse
{
    public string? About { get; set; }
    public string LogoUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public int? PoolCount { get; set; }
    public string ExperienceInYears { get; set; }
    public string Name { get; set; }
    public string PhoneNumber { get; set; }
    public string PreferredCommunicationsEmail { get; set; }
    public BusinessLocationResponse Location { get; set; }
    public List<JobCategoryResponse> Categories { get; set; }
}
