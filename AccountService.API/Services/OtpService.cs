using System;
using System.Threading.Tasks;
using AccountService.API.Config;
using AccountService.API.Config.Interfaces;
using AccountService.API.Constants;
using AccountService.API.Dto;
using AccountService.API.Dto.NotificationServiceClient;
using AccountService.API.Dto.Request;
using AccountService.API.Enums;
using AccountService.API.Models;
using AccountService.API.Services.Interfaces;
using AccountService.API.Utils;
using AccountService.API.Utils.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using PodCommonsLibrary.Core.Exceptions;

namespace AccountService.API.Services;

public class OtpService : IOtpService
{
    private readonly AppConfig _appConfig;
    private readonly IDistributedCache _cache;
    private readonly ICustomerService _customerService;
    private readonly IServiceBusConfig _serviceBusConfig;

    public OtpService(
        ICustomerService customerService,
        IDistributedCache cache,
        AppConfig appConfig,
        IServiceBusConfig serviceBusConfig
    )
    {
        _cache = cache;
        _appConfig = appConfig;
        _customerService = customerService;
        _serviceBusConfig = serviceBusConfig;
    }

    public async Task<Session> GenerateOtp(Session sessionRequest)
    {
        CustomerSession customerSession = await _customerService.GetCustomerSession(sessionRequest.SessionId);
        string otp = ValidateAndGetOtp(customerSession);
        customerSession.Otp = otp;
        customerSession.OtpGenerationDateTime = DateTime.UtcNow;

        EmailRequestWithTemplateParameters emailRequestWithTemplateParameters = new()
        {
            RecipientAddress = customerSession.Email,
            SenderAddress = _appConfig.OtpEmailSenderAddress,
            Subject = StaticValues.EmailTemplateConfig[customerSession.Type]["Subject"],
            HtmlTemplateName = StaticValues.EmailTemplateConfig[customerSession.Type]["Template"],
            HtmlTemplateParameters = await GetHtmlTemplateParameters(customerSession)
        };

        string message = JsonConvert.SerializeObject(emailRequestWithTemplateParameters);
        await _serviceBusConfig.SendEmailMessageToQueue(message);
        await _cache.SetAsync(sessionRequest.SessionId, customerSession.ObjectToByteArray());
        return sessionRequest;
    }

    public async Task<Session> ValidateOtp(ValidateOtpRequest validateOtpRequest)
    {
        CustomerSession customerSession = await _customerService.GetCustomerSession(validateOtpRequest.SessionId);
        if (customerSession.Otp == null || customerSession.Otp != validateOtpRequest.Otp)
        {
            throw new BusinessRuleViolationException(StaticValues.InvalidOtp, StaticValues.ErrorIncorrectOtp);
        }

        if (customerSession.OtpGenerationDateTime != null)
        {
            bool isOtpExpired = customerSession.OtpGenerationDateTime.Value.AddDays(_appConfig.OtpValidityInDays) <
                                DateTime.UtcNow;
            if (isOtpExpired)
            {
                throw new BusinessRuleViolationException(StaticValues.OtpExpired, StaticValues.ErrorOtpExpired);
            }
        }

        customerSession.EmailVerified = true;
        customerSession.OtpVerified = true;
        await _cache.SetAsync(validateOtpRequest.SessionId, customerSession.ObjectToByteArray());
        return new Session
        {
            SessionId = validateOtpRequest.SessionId
        };
    }

    private string ValidateAndGetOtp(CustomerSession customerSession)
    {
        if (customerSession.Otp != null && customerSession.OtpGenerationDateTime != null)
        {
            DateTime currentDateTime = DateTime.UtcNow;
            DateTime lastOtpSentDateTime = customerSession.OtpGenerationDateTime.Value;
            double durationInSeconds = currentDateTime.Subtract(lastOtpSentDateTime).TotalSeconds;
            if (durationInSeconds < _appConfig.OtpCoolDownTimeInSeconds)
            {
                throw new BusinessRuleViolationException(StaticValues.OtpCoolDownError,
                    StaticValues.ErrorOtpCoolDown(_appConfig.OtpCoolDownTimeInSeconds - durationInSeconds));
            }
        }

        // random n digit number
        string otp = NumberUtils.GenerateRandomNDigits((int) _appConfig.OtpLength);
        return otp;
    }


    private async Task<dynamic> GetTemplateParametersForReset(CustomerSession customerSession)
    {
        Customer customer = await _customerService.GetCustomerByAccountId(customerSession.Email);
        dynamic emailOtpVerification = new
        {
            customerSession.Otp,
            Validity = $"{_appConfig.OtpValidityInDays}",
            customer.Account.FirstName
        };
        return emailOtpVerification;
    }

    private dynamic GetTemplateParametersForRegister(CustomerSession customerSession)
    {
        dynamic emailOtpVerification = new
        {
            customerSession.Otp,
            Validity = $"{_appConfig.OtpValidityInDays}",
            customerSession.CustomerRegistrationRequest?.FirstName
        };
        return emailOtpVerification;
    }

    private async Task<dynamic> GetHtmlTemplateParameters(CustomerSession customerSession)
    {
        return customerSession.Type switch
        {
            EmailTemplateEnum.Register => GetTemplateParametersForRegister(customerSession),
            EmailTemplateEnum.Reset => await GetTemplateParametersForReset(customerSession),
            _ => throw new NotFoundException(StaticValues.HtmlTemplateParametersNotFound)
        };
    }
}
