using System.Threading.Tasks;
using AccountService.API.Controllers;
using AccountService.API.Dto;
using AccountService.API.Dto.Request;
using AccountService.API.Services.Interfaces;
using AutoFixture;
using AutoFixture.DataAnnotations;
using Moq;
using Xunit;

namespace AccountService.API.Tests.Controllers;

public class AuthControllerTests
{
    private readonly AuthController _authController;
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<ICustomerService> _customerServiceMock;
    private readonly IFixture _fixture;

    public AuthControllerTests()
    {
        _fixture = new Fixture();
        _fixture.Customizations.Add(new RegularExpressionGenerator());
        _fixture.Customizations.Add(new RegularExpressionAttributeRelay());
        _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _customerServiceMock = new Mock<ICustomerService>();
        _authServiceMock = new Mock<IAuthService>();
        _authController = new AuthController(_authServiceMock.Object, _customerServiceMock.Object);
    }

    [Fact(DisplayName =
        "AuthController: CreateCustomerSession - Should call create customer session function in customer service to create a new session and return session id.")]
    public async Task CreateCustomerSession_Success()
    {
        CustomerRegistrationRequest customerSessionRequestMock = _fixture.Create<CustomerRegistrationRequest>();
        Session sessionMock = _fixture.Create<Session>();
        _customerServiceMock.Setup(x => x.CreateCustomerSession(customerSessionRequestMock))
            .ReturnsAsync(sessionMock);

        Session result = await _authController.CreateCustomerSession(customerSessionRequestMock);

        // assert
        Assert.IsType<Session>(result);
        Assert.Equal(sessionMock, result);
    }


    [Fact(DisplayName =
        "AuthController: ChangeCustomerPassword - Should successfully change the customer password.")]
    public async Task ChangeCustomerPassword_Success()
    {
        ChangeCustomerPasswordRequest changeCustomerPasswordRequestMock =
            _fixture.Create<ChangeCustomerPasswordRequest>();
        _authServiceMock.Setup(x => x.ChangeCustomerPassword(changeCustomerPasswordRequestMock, "customer@mail.com"));

        await _authController.ChangeCustomerPassword(changeCustomerPasswordRequestMock, "customer@mail.com");

        // assert
        _authServiceMock.Verify(x => x.ChangeCustomerPassword(changeCustomerPasswordRequestMock, "customer@mail.com"),
            Times.Once);
    }
}
