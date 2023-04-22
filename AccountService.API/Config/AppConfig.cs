using System;
using Microsoft.Extensions.Configuration;

namespace AccountService.API.Config;

public class AppConfig
{
    public AppConfig(IConfiguration configuration)
    {
        OtpValidityInDays = Convert.ToDouble(configuration[nameof(OtpValidityInDays)]);
        OtpCoolDownTimeInSeconds = Convert.ToDouble(configuration[nameof(OtpCoolDownTimeInSeconds)]);
        OtpLength = Convert.ToDouble(configuration[nameof(OtpLength)]);
        SessionValidityInDays = Convert.ToDouble(configuration[nameof(SessionValidityInDays)]);
        OtpEmailSenderAddress = configuration[nameof(OtpEmailSenderAddress)];
        DstEmailAddress = configuration[nameof(DstEmailAddress)];
        SenderEmailAddress = configuration[nameof(SenderEmailAddress)];
    }

    public double OtpValidityInDays { get; }
    public double OtpCoolDownTimeInSeconds { get; }
    public double OtpLength { get; }
    public double SessionValidityInDays { get; }
    public string OtpEmailSenderAddress { get; }
    public string DstEmailAddress { get; }
    public string SenderEmailAddress { get; }
}
