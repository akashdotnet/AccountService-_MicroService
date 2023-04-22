using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using AccountService.API.Constants;
using AccountService.API.Enums;

namespace AccountService.API.Validations;

public class EmailAttribute : ValidationAttribute
{
    private readonly EmailTypeEnum _emailType;

    public EmailAttribute(EmailTypeEnum emailType)
    {
        _emailType = emailType;
    }

    public override bool IsValid(object? value)
    {
        if (value is string email)
        {
            // 1. general email validation
            bool isEmailValid = new EmailAddressAttribute().IsValid(email);
            if (isEmailValid)
            {
                // 2. role specific validation
                MailAddress eMail = new(email);
                string hostName = eMail.Host;

                return _emailType switch
                {
                    EmailTypeEnum.Pentair => hostName == StaticValues.PentairEmailHostName,
                    EmailTypeEnum.NonPentair => hostName != StaticValues.PentairEmailHostName,
                    _ => false
                };
            }
        }

        return false;
    }

    public override string FormatErrorMessage(string name)
    {
        if (_emailType == EmailTypeEnum.Pentair)
        {
            return StaticValues.ErrorInvalidPentairEmail;
        }

        return StaticValues.ErrorInvalidNonPentairEmail;
    }
}
