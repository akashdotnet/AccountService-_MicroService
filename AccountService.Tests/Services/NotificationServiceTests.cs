using AccountService.API.Config;
using AccountService.API.Config.Interfaces;
using AccountService.API.Models;
using AccountService.API.Services;
using AutoFixture;
using Microsoft.Extensions.Configuration;
using Moq;
using PodCommonsLibrary.Core.Utils.AutoFixture;
using Xunit;

namespace AccountService.API.Tests.Services;

public class NotificationServiceTests
{
    private readonly Mock<AppConfig> _appConfig;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly IFixture _fixture;
    private readonly NotificationService _notificationService;
    private readonly Mock<IServiceBusConfig> _serviceBusConfigMock;

    public NotificationServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _fixture.Customizations.Add(new DateOnlyBuilder());

        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.SetupGet(x =>
                x[It.Is<string>(s => s == "DstEmailAddress")])
            .Returns("servicercontact@rippleondemand.com");
        _configurationMock.SetupGet(x =>
                x[It.Is<string>(s => s == "SenderEmailAddress")])
            .Returns("ripple@rippleondemand.com");

        _appConfig = new Mock<AppConfig>(_configurationMock.Object);
        _serviceBusConfigMock = new Mock<IServiceBusConfig>();

        _notificationService = new NotificationService(
            _appConfig.Object,
            _serviceBusConfigMock.Object
        );
    }

    [Fact(DisplayName =
        "NotificationService : NotifyDstOnDealerSignUp - Should successfully send email to dealer success team when a new dealer signs up.")]
    public async void NotifyDstOnDealerSignUp_Success()
    {
        //arrange
        Dealer dealerMock = _fixture.Create<Dealer>();
        _serviceBusConfigMock.Setup(x => x.SendEmailMessageToQueue(It.IsAny<string>()));

        //act
        await _notificationService.NotifyDstOnDealerSignUp(dealerMock);

        //assert
        _serviceBusConfigMock.Verify(x => x.SendEmailMessageToQueue(It.IsAny<string>()),
            Times.Exactly(1));
    }
}
