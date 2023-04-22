using AccountService.API.Constants;
using AccountService.API.Utils;
using PodCommonsLibrary.Core.Exceptions;
using Xunit;

namespace AccountService.API.Tests.Utils;

public class YearUtilsTests
{
    [Theory(DisplayName =
        "YearUtilsTests: GetStartYearAsInteger - Should successfully get the start year as integer.")]
    [InlineData("2006", 2024, 2006)]
    [InlineData("Before 2000", 2024, 1999)]
    [InlineData("2018", 2024, 2018)]
    public void GetStartYearAsInteger_Success(string startYear, int currentYear, int expectedResult)
    {
        int result = YearUtils.GetStartYearAsInteger(startYear, currentYear);

        Assert.Equal(expectedResult, result);
    }

    [Fact(DisplayName =
        "YearUtilsTests: GetStartYearAsInteger - Should throw business violation exception if the given start year is greater than current year.")]
    public void GetStartYearAsInteger_StartYearGreaterThanCurrentYear_ThrowsBusinessRuleViolationException()
    {
        BusinessRuleViolationException businessRuleViolationException =
            Assert.Throws<BusinessRuleViolationException>(
                () => { YearUtils.GetStartYearAsInteger("2026", 2024); });

        Assert.Equal(StaticValues.InvalidStartYear, businessRuleViolationException.ErrorResponseType);
        Assert.Equal("2026 is not a valid start year. Valid values are 2000-2024 or Before 2000",
            businessRuleViolationException.Message);
    }

    [Fact(DisplayName =
        "YearUtilsTests: GetStartYearAsInteger - Should throw business violation exception if the given start year is invalid.")]
    public void GetStartYearAsInteger_InvalidStartYear_ThrowsBusinessRuleViolationException()
    {
        BusinessRuleViolationException businessRuleViolationException =
            Assert.Throws<BusinessRuleViolationException>(
                () => { YearUtils.GetStartYearAsInteger("random-value", 2024); });

        Assert.Equal(StaticValues.InvalidStartYear, businessRuleViolationException.ErrorResponseType);
        Assert.Equal("random-value is not a valid start year. Valid values are 2000-2024 or Before 2000",
            businessRuleViolationException.Message);
    }

    [Theory(DisplayName =
        "YearUtilsTests: GetStartYearOptionValue - Should successfully get the start year to be shown in the option value from integer start year.")]
    [InlineData(2006, "2006")]
    [InlineData(1999, "Before 2000")]
    [InlineData(null, null)]
    public void GetStartYearOptionValue_Success(int? startYear, string expectedResult)
    {
        string? result = YearUtils.GetStartYearOptionValue(startYear);

        Assert.Equal(expectedResult, result);
    }

    [Theory(DisplayName =
        "YearUtilsTests: GetYearsOfExperienceFromStartYear - Should successfully get the years of experience from the start year.")]
    [InlineData(2006, 2026, "20")]
    [InlineData(1999, 2026, "27+")]
    [InlineData(null, 2026, null)]
    public void GetYearsOfExperienceFromStartYear_Success(int? startYear, int currentYear, string expectedResult)
    {
        string? result = YearUtils.GetYearsOfExperienceFromStartYear(startYear, currentYear);

        Assert.Equal(expectedResult, result);
    }
}
