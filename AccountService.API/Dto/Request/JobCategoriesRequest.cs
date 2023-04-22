using AccountService.API.Enums;

namespace AccountService.API.Dto.Request;

public class JobCategoriesRequest : LookupCodesBaseModel
{
    public JobCategoryTypeEnum Type { get; set; }
}
