using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace AccountService.API.Validations;

public class RegularExpressionListAttribute : RegularExpressionAttribute
{
    public RegularExpressionListAttribute(string pattern)
        : base(pattern)
    {
    }

    public override bool IsValid(object? value)
    {
        if (value is not IEnumerable<string>)
        {
            return false;
        }

        List<string> stringValues = (List<string>) value;
        foreach (string val in stringValues)
        {
            if (!Regex.IsMatch(val, Pattern))
            {
                return false;
            }
        }

        return true;
    }
}
