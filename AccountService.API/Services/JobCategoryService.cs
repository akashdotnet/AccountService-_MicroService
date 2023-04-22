using System.Collections.Generic;
using System.Linq;
using AccountService.API.Constants;
using AccountService.API.Dto.Request;
using AccountService.API.Models;

namespace AccountService.API.Services;

public static class JobCategoryService
{
    public static List<BusinessJobCategory> CreateOrUpdateJobCategories(
        List<BusinessJobCategory> existingBusinessJobCategories,
        List<JobCategoriesRequest> incomingBusinessJobCategories)
    {
        List<string> incomingJobCategoryCodes =
            incomingBusinessJobCategories?.SelectMany(x => x.Codes)?.ToList() ?? new List<string>();
        List<string> otherJobCategoryCodes = new()
            {StaticValues.JobCategoryRepairOtherName, StaticValues.JobCategoryServiceOtherName};

        IEnumerable<BusinessJobCategory> businessJobCategories = new HashSet<string>(incomingJobCategoryCodes).Select(
            incomingJobCode =>
            {
                string? othersData = incomingBusinessJobCategories?
                    .FirstOrDefault(x => x.Codes.Contains(incomingJobCode)
                                         && otherJobCategoryCodes.Contains(incomingJobCode))?.Others;
                //use the same object if the brand code is already mapped to business
                BusinessJobCategory businessJobCategory = existingBusinessJobCategories.Find(existingJobCategory =>
                                                              existingJobCategory.Code == incomingJobCode)
                                                          ?? new BusinessJobCategory
                                                          {
                                                              Code = incomingJobCode
                                                          };
                businessJobCategory.Others = othersData;
                return businessJobCategory;
            }
        );
        return businessJobCategories.ToList();
    }
}
