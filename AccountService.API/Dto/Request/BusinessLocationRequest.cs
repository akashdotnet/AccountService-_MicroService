using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AccountService.API.Constants;

namespace AccountService.API.Dto.Request;

public class BusinessLocationRequest
{
    public int? Id { get; set; }

    [MaxLength(StaticValues.DefaultMaxLength)]
    public string OfficeName { get; set; }

    public DealerAddressRequest Address { get; set; }
    public List<string>? ServiceableCounties { get; set; }
}
