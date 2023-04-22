using System.Threading.Tasks;
using AccountService.API.Constants;
using AccountService.API.Dto;
using AccountService.API.Dto.Request;
using AccountService.API.Services.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PodCommonsLibrary.Core.Dto;

namespace AccountService.API.Controllers;

[ApiController]
[Route(StaticValues.OtpControllerRoutePrefix)]
public class OtpController : ControllerBase
{
    private readonly IOtpService _otpService;

    public OtpController(IOtpService otpService, IMapper mapper)
    {
        _otpService = otpService;
    }

    [HttpPost(StaticValues.GeneratePath)]
    [ProducesResponseType(typeof(Session), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<Session> GenerateOtp([FromBody] Session sessionRequest)
    {
        return await _otpService.GenerateOtp(sessionRequest);
    }


    [HttpPost(StaticValues.ValidatePath)]
    [ProducesResponseType(typeof(Session), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<Session> ValidateOtp([FromBody] ValidateOtpRequest validateOtpRequest)
    {
        return await _otpService.ValidateOtp(validateOtpRequest);
    }
}
