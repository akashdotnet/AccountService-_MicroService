using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AccountService.API.Enums;

namespace AccountService.API.Dto.Request;

public class UpdateCustomerRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    [Phone] public string? PhoneNumber { get; set; }

    public CustomerAddressRequest? Address { get; set; }
    public string? SanitationMethodCode { get; set; }
    public string? PoolTypeCode { get; set; }
    public string? PoolSizeCode { get; set; }
    public string? PoolMaterialCode { get; set; }
    public string? HotTubTypeCode { get; set; }
    public string? PoolSeasonCode { get; set; }
    public List<string>? HearAboutSurveys { get; set; }
    [Required] public CustomerStepEnum OnboardingStep { get; set; }
}
