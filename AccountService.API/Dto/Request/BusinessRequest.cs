using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AccountService.API.Constants;

namespace AccountService.API.Dto.Request;

public class BusinessRequest
{
    [MaxLength(StaticValues.DefaultMaxLength)]
    public string? Name { get; set; }

    public List<BusinessLocationRequest>? Locations { get; set; }

    [MaxLength(StaticValues.AboutMaxLength)]
    public string? About { get; set; }

    public BusinessBrandRequest? Brands { get; set; }
    public List<JobCategoriesRequest>? Categories { get; set; }
    public string? WebsiteUrl { get; set; }
    public int? PoolCount { get; set; }
    public string? StartYear { get; set; }
    [Phone] public string? PhoneNumber { get; set; }
    [EmailAddress] public string? PreferredCommunicationsEmail { get; set; }
}
