using System;
using System.Threading.Tasks;
using AccountService.API.Config;
using AccountService.API.Config.Interfaces;
using AccountService.API.Dto;
using AccountService.API.Dto.NotificationServiceClient;
using AccountService.API.Dto.Request;
using AccountService.API.Enums;
using AccountService.API.Models;
using AccountService.API.Services;
using AccountService.API.Services.Interfaces;
using AutoFixture;
using AutoFixture.DataAnnotations;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using PodCommonsLibrary.Core.Exceptions;
using Xunit;

namespace AccountService.API.Tests.Services;

public class OtpServiceTests
{
    private readonly Mock<IDistributedCache> _cache;
    private readonly Mock<ICustomerService> _customerServiceMock;
    private readonly IFixture _fixture;
    private readonly Mock<DistributedCacheEntryOptions> _mockDistributedCacheEntryOptions;
    private readonly OtpService _otpService;
    private readonly Mock<IServiceBusConfig> _serviceBusConfigMock;
    private readonly Mock<AppConfig> appConfigMock;
    private readonly Mock<IConfiguration> mockConfig;

    public OtpServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Customizations.Add(new RegularExpressionGenerator());
        _fixture.Customizations.Add(new RegularExpressionAttributeRelay());
        _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _cache = new Mock<IDistributedCache>();
        _mockDistributedCacheEntryOptions = new Mock<DistributedCacheEntryOptions>();
        _customerServiceMock = new Mock<ICustomerService>();
        mockConfig = new Mock<IConfiguration>();
        _serviceBusConfigMock = new Mock<IServiceBusConfig>();
        mockConfig.SetupGet(x => x[It.Is<string>(s => s == "OtpLength")]).Returns("6");
        mockConfig.SetupGet(x => x[It.Is<string>(s => s == "OtpCoolDownTimeInSeconds")]).Returns("60");
        mockConfig.SetupGet(x => x[It.Is<string>(s => s == "OtpValidityInDays")]).Returns("10");
        appConfigMock = new Mock<AppConfig>(mockConfig.Object);
        _otpService = new OtpService(
            _customerServiceMock.Object,
            _cache.Object,
            appConfigMock.Object,
            _serviceBusConfigMock.Object
        );
        _cache.Setup(x =>
                x.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), _mockDistributedCacheEntryOptions.Object, default))
            .Returns(Task.FromResult(true));
    }

    [Fact(DisplayName = "Generate OTP Success for Reset password")]
    public async Task GenerateOtp_SuccessReset()
    {
        // arrange
        EmailRequestWithTemplateParameters? emailRequestWithTemplateParameters = null;
        Session mockSession = _fixture.Create<Session>();
        mockSession.SessionId = "1234";
        CustomerSession customerSession = _fixture.Build<CustomerSession>()
            .With(x => x.Email, "abc@mail.com")
            .With(x => x.Type, EmailTemplateEnum.Reset)
            .With(x => x.OtpGenerationDateTime, new DateTime(2020, 1, 1))
            .Create();
        Customer customerMock = _fixture.Create<Customer>();
        _customerServiceMock.Setup(u => u
                .GetCustomerSession(It.IsAny<string>()))
            .ReturnsAsync(customerSession);
        _customerServiceMock.Setup(x => x.GetCustomerByAccountId(customerSession.Email))
            .ReturnsAsync(customerMock);
        _serviceBusConfigMock.Setup(x => x.SendEmailMessageToQueue(It.IsAny<string>()))
            .Callback((string emailRequestAsString) =>
            {
                emailRequestWithTemplateParameters =
                    JsonConvert.DeserializeObject<EmailRequestWithTemplateParameters>(emailRequestAsString);
            });
        // act
        Session actualOutput = await _otpService.GenerateOtp(mockSession);

        // assert
        Assert.Equal(mockSession.SessionId, actualOutput.SessionId);
        Assert.NotNull(emailRequestWithTemplateParameters);
        Assert.Equal("Reset password for Ripple account", emailRequestWithTemplateParameters?.Subject);
        Assert.Equal("ResetEmailTemplate.html", emailRequestWithTemplateParameters?.HtmlTemplateName);
        Assert.Equal(customerSession.Otp, emailRequestWithTemplateParameters?.HtmlTemplateParameters.Otp.ToString());
        Assert.Equal(customerMock.Account.FirstName,
            emailRequestWithTemplateParameters?.HtmlTemplateParameters.FirstName.ToString());
    }

    [Fact(DisplayName = "Generate OTP Success for Register user")]
    public async Task GenerateOtp_SuccessRegister()
    {
        // arrange
        Session mockSession = _fixture.Create<Session>();
        mockSession.SessionId = "1234";
        CustomerSession customerSession = _fixture.Build<CustomerSession>()
            .With(x => x.Email, "abc@mail.com")
            .With(x => x.Type, EmailTemplateEnum.Register)
            .With(x => x.OtpGenerationDateTime, new DateTime(2020, 1, 1))
            .With(x => x.CustomerRegistrationRequest, new CustomerRegistrationRequest
            {
                FirstName = "abc",
                LastName = "def"
            })
            .Create();
        _customerServiceMock.Setup(u => u
                .GetCustomerSession(It.IsAny<string>()))
            .ReturnsAsync(customerSession);
        // act
        Session actualOutput = await _otpService.GenerateOtp(mockSession);

        // assert
        Assert.Equal(mockSession.SessionId, actualOutput.SessionId);
    }

    [Fact(DisplayName = "Generate OTP Failure: Cooldown not expired")]
    public async Task GenerateOtp_OtpCooldown()
    {
        // arrange
        Session mockSession = _fixture.Create<Session>();
        mockSession.SessionId = "1234";
        CustomerSession customerSession = _fixture.Build<CustomerSession>()
            .With(x => x.Email, "abc@mail.com")
            .With(x => x.Type, EmailTemplateEnum.Reset)
            .With(x => x.OtpGenerationDateTime, DateTime.Now.AddDays(10))
            .Create();
        _customerServiceMock.Setup(u => u
                .GetCustomerSession(It.IsAny<string>()))
            .ReturnsAsync(customerSession);
        // Act
        BusinessRuleViolationException businessRuleViolationException =
            await Assert.ThrowsAsync<BusinessRuleViolationException>(async () =>
                await _otpService.GenerateOtp(mockSession));

        // assert
        Assert.StartsWith("An OTP has already been sent",
            businessRuleViolationException.Message);
    }

    [Fact(DisplayName = "Validate OTP Success")]
    public async Task ValidateOtp_Success()
    {
        // arrange
        ValidateOtpRequest mockRequest = _fixture.Create<ValidateOtpRequest>();
        mockRequest.Otp = "123456";
        mockRequest.SessionId = "1234";
        CustomerSession customerSession = _fixture.Build<CustomerSession>()
            .With(x => x.Otp, "123456")
            .With(x => x.Type, EmailTemplateEnum.Reset)
            .Create();
        customerSession.OtpGenerationDateTime = null;
        _customerServiceMock.Setup(u => u
                .GetCustomerSession(It.IsAny<string>()))
            .ReturnsAsync(customerSession);
        // act
        Session actualOutput = await _otpService.ValidateOtp(mockRequest);

        // assert
        Assert.Equal(mockRequest.SessionId, actualOutput.SessionId);
    }

    [Fact(DisplayName = "Validate OTP Failure: OTP mismatch")]
    public async Task ValidateOtp_OtpMismatch()
    {
        // arrange
        ValidateOtpRequest mockRequest = _fixture.Create<ValidateOtpRequest>();
        mockRequest.Otp = "123456";
        mockRequest.SessionId = "1234";
        CustomerSession customerSession = _fixture.Build<CustomerSession>()
            .With(x => x.Otp, "123457")
            .With(x => x.Type, EmailTemplateEnum.Reset)
            .Create();
        customerSession.OtpGenerationDateTime = null;
        _customerServiceMock.Setup(u => u
                .GetCustomerSession(It.IsAny<string>()))
            .ReturnsAsync(customerSession);
        // Act
        BusinessRuleViolationException businessRuleViolationException =
            await Assert.ThrowsAsync<BusinessRuleViolationException>(async () =>
                await _otpService.ValidateOtp(mockRequest));

        // assert
        Assert.Equal("Invalid OTP",
            businessRuleViolationException.Message);
    }

    [Fact(DisplayName = "Validate OTP Failure: OTP expired")]
    public async Task ValidateOtp_OtpExpired()
    {
        // arrange
        ValidateOtpRequest mockRequest = _fixture.Create<ValidateOtpRequest>();
        mockRequest.Otp = "123456";
        mockRequest.SessionId = "1234";
        CustomerSession customerSession = _fixture.Build<CustomerSession>()
            .With(x => x.Otp, "123456")
            .With(x => x.Type, EmailTemplateEnum.Reset)
            .Create();
        customerSession.OtpGenerationDateTime = DateTime.Now.AddDays(-15);
        _customerServiceMock.Setup(u => u
                .GetCustomerSession(It.IsAny<string>()))
            .ReturnsAsync(customerSession);
        // Act
        BusinessRuleViolationException businessRuleViolationException =
            await Assert.ThrowsAsync<BusinessRuleViolationException>(async () =>
                await _otpService.ValidateOtp(mockRequest));

        // assert
        Assert.Equal("The OTP has expired, please generate a new OTP",
            businessRuleViolationException.Message);
    }
}
