using System.Collections.Generic;
using System.Linq;
using AccountService.API.Constants;
using AccountService.API.Dto.Request;
using AccountService.API.Models;

namespace AccountService.API.Services;

public static class LanguageService
{
    public static List<ExpertLanguage> CreateOrUpdateExpertLanguages(List<ExpertLanguage> existingExpertLanguages,
        LanguageRequest? incomingLanguages)
    {
        if (incomingLanguages == null)
        {
            return existingExpertLanguages;
        }

        IEnumerable<ExpertLanguage> expertLanguages = new HashSet<string>(incomingLanguages.Codes).Select(
            incomingLanguageCode =>
            {
                string? othersData = incomingLanguages.Codes?
                                         .FirstOrDefault(x => x.Contains(StaticValues.OthersCode)) != null
                                     && incomingLanguageCode == StaticValues.OthersCode
                    ? incomingLanguages.Others
                    : null;
                //use the same object if the brand code is already mapped to business
                ExpertLanguage expertLanguageObj = existingExpertLanguages.Find(existingExpertLanguage =>
                                                       existingExpertLanguage.Code == incomingLanguageCode)
                                                   ?? new ExpertLanguage
                                                   {
                                                       Code = incomingLanguageCode
                                                   };
                expertLanguageObj.Others = othersData;
                return expertLanguageObj;
            }
        );

        return expertLanguages.ToList();
    }
}
