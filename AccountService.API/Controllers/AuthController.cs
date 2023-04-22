using System.Threading.Tasks;
using AccountService.API.Constants;
using AccountService.API.Dto;
using AccountService.API.Dto.Request;
using AccountService.API.Dto.Response;
using AccountService.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PodCommonsLibrary.Core.Annotations;
using PodCommonsLibrary.Core.Dto;
using PodCommonsLibrary.Core.Enums;

namespace AccountService.API.Controllers;

[ApiController]
[Route(StaticValues.ApiRoutePrefix)]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICustomerService _customerService;

    public AuthController(
        IAuthService authService,
        ICustomerService customerService
    )
    {
        _authService = authService;
        _customerService = customerService;
    }

    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    [HttpPost(StaticValues.SignUpCustomerPath)]
    public async Task<ActionResult<CustomerResponse>> RegisterCustomer(
        [FromBody] Session session)
    {
        CustomerRegistrationRequest customerRegistrationRequest =
            await _customerService.GetCustomerRegistrationRequestAndValidateEmailVerification(session);
        await _authService.CreateIdentityUser(customerRegistrationRequest, UserRoleEnum.Customer);
        return StatusCode(StatusCodes.Status201Created,
            await _customerService.CreateCustomer(customerRegistrationRequest, session));
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    [HttpPost(StaticValues.LoginPath)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest loginRequest)
    {
        LoginResponse loginResponse = await _authService.GenerateToken(loginRequest);
        return StatusCode(StatusCodes.Status200OK, loginResponse);
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    [HttpPost(StaticValues.RefreshTokenPath)]
    public async Task<ActionResult<LoginResponse>> RefreshToken([FromBody] RefreshTokenRequest refreshTokenRequest)
    {
        LoginResponse loginResponse = await _authService.RefreshToken(refreshTokenRequest);
        return StatusCode(StatusCodes.Status200OK, loginResponse);
    }


    [HttpPost(StaticValues.CustomerSessionPath)]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<Session> CreateCustomerSession(
        [FromBody] CustomerRegistrationRequest customerRegistrationRequest)
    {
        return await _customerService.CreateCustomerSession(customerRegistrationRequest);
    }

    [HttpPost(StaticValues.CustomerResetPasswordTokenPath)]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Session>> ResetPasswordToken(
        [FromBody] CustomerResetPasswordRequest customerResetPasswordRequest)
    {
        Session session = await _authService.GenerateResetToken(customerResetPasswordRequest);
        return StatusCode(StatusCodes.Status200OK, session);
    }

    [HttpPost(StaticValues.CustomerPasswordPath)]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IdentityResult>> SetNewPassword(
        [FromBody] CustomerSetNewPassword customerSetNewPassword)
    {
        IdentityResult resetStatus = await _authService.SetNewPassword(customerSetNewPassword);
        return StatusCode(StatusCodes.Status200OK, resetStatus);
    }

    [Authorize(UserRoleEnum.Customer)]
    [HttpPut(StaticValues.CustomerPasswordPath)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task ChangeCustomerPassword(
        [FromBody] ChangeCustomerPasswordRequest changeCustomerPasswordRequest,
        [FromHeader(Name = StaticValues.EmailHeader)]
        string customerEmail
    )
    {
        await _authService.ChangeCustomerPassword(changeCustomerPasswordRequest, customerEmail);
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    [HttpPost(StaticValues.LogoutCustomerPath)]
    public async Task<ActionResult> LogoutCustomer([FromBody] RefreshTokenRequest refreshTokenRequest)
    {
        await _authService.LogoutCustomer(refreshTokenRequest);
        return StatusCode(StatusCodes.Status200OK);
    }
}
