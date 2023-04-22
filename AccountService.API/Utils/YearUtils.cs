using AccountService.API.Constants;
using PodCommonsLibrary.Core.Exceptions;

namespace AccountService.API.Utils;

public class YearUtils
{
    // passing the year as argument to ensure the following functions are testable
    public static int GetStartYearAsInteger(string startYear, int currentYear)
    {
        if (int.TryParse(startYear, out int startYearAsInteger) && startYearAsInteger <= currentYear)
        {
            return startYearAsInteger;
        }

        if (startYear == StaticValues.MinDealerStartYearOptionValue)
        {
            return StaticValues.MinDealerStartYear - 1;
        }

        throw new BusinessRuleViolationException(StaticValues.InvalidStartYear,
            StaticValues.ErrorInvalidStartYear(currentYear, startYear));
    }

    public static string? GetStartYearOptionValue(int? startYear)
    {
        if (startYear is null)
        {
            return null;
        }

        return startYear >= StaticValues.MinDealerStartYear
            ? startYear.ToString()
            : StaticValues.MinDealerStartYearOptionValue;
    }

    public static string? GetYearsOfExperienceFromStartYear(int? startYear, int currentYear)
    {
        if (startYear is null)
        {
            return null;
        }

        int yearsOfExperience = currentYear - (int) startYear;
        return startYear >= StaticValues.MinDealerStartYear
            ? yearsOfExperience.ToString()
            : $"{yearsOfExperience}+";
    }
}
