using System.ComponentModel.DataAnnotations;
using AccountService.API.Constants;

namespace AccountService.API.Dto.Request;

public class BaseAddressRequest
{
    [MaxLength(StaticValues.DefaultMaxLength)]
    public string AddressValue { get; set; }

    [MaxLength(StaticValues.DefaultMaxLength)]
    public string City { get; set; }

    [MaxLength(StaticValues.DefaultMaxLength)]
    public string State { get; set; }

    [RegularExpression(StaticValues.ZipCodeRegex, ErrorMessage = StaticValues.ErrorInvalidZipCode)]
    public string ZipCode { get; set; }
}
