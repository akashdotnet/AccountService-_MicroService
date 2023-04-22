using System.Collections.Generic;
using System.Linq;
using AccountService.API.Constants;
using AccountService.API.Dto.Request;
using AccountService.API.Models;

namespace AccountService.API.Services;

public static class SkillService
{
    public static List<ExpertSkill> CreateOrUpdateExpertSkills(List<ExpertSkill> existingExpertSkills,
        SkillRequest? incomingExpertSkills)
    {
        if (incomingExpertSkills == null)
        {
            return existingExpertSkills;
        }

        IEnumerable<ExpertSkill> expertSkills = new HashSet<string>(incomingExpertSkills.Codes).Select(
            incomingSkillCode =>
            {
                string? othersData = incomingExpertSkills.Codes?
                                         .FirstOrDefault(x => x.Contains(StaticValues.OthersCode)) != null
                                     && incomingSkillCode == StaticValues.OthersCode
                    ? incomingExpertSkills.Others
                    : null;
                //use the same object if the brand code is already mapped to business
                ExpertSkill expertSkillObj = existingExpertSkills.Find(existingExpertSkill =>
                                                 existingExpertSkill.Code == incomingSkillCode)
                                             ?? new ExpertSkill
                                             {
                                                 Code = incomingSkillCode
                                             };
                expertSkillObj.Others = othersData;
                return expertSkillObj;
            }
        );

        return expertSkills.ToList();
    }
}
