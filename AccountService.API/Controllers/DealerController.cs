using System.Collections.Generic;
using System.Threading.Tasks;
using AccountService.API.Constants;
using AccountService.API.Dto.Request;
using AccountService.API.Dto.Response;
using AccountService.API.Models;
using AccountService.API.Services.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PodCommonsLibrary.Core.Annotations;
using PodCommonsLibrary.Core.Dto;
using PodCommonsLibrary.Core.Enums;

namespace AccountService.API.Controllers;

[ApiController]
[Route(StaticValues.DealerControllerRoutePrefix)]
public class DealerController : ControllerBase
{
    private readonly IDealerService _dealerService;
    private readonly IMapper _mapper;

    public DealerController(IDealerService dealerService, IMapper mapper)
    {
        _dealerService = dealerService;
        _mapper = mapper;
    }

    [HttpPut]
    [Authorize(UserRoleEnum.Dealer)]
    [ProducesResponseType(typeof(DealerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<DealerResponse> UpdateDealer([FromBody] UpdateDealerRequest updateDealerRequest,
        [FromHeader(Name = StaticValues.UserTypeHeader)]
        string userType)
    {
        Dealer dealer = await _dealerService.UpdateDealer(updateDealerRequest, userType);
        return _mapper.Map<DealerResponse>(dealer);
    }

    [HttpGet]
    [Authorize(UserRoleEnum.Dealer)]
    [ProducesResponseType(typeof(DealerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<DealerResponse> GetDealer()
    {
        return await _dealerService.GetDealerWithBusinessLogo();
    }

    [HttpGet(StaticValues.DealerByBusinessLocationInternalPath)]
    [ProducesResponseType(typeof(List<DealerResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<List<DealerResponse>> GetDealerByBusinessLocationIds(
        [FromQuery] List<int> locationIds)
    {
        return await _dealerService.GetDealerByBusinessLocationIds(locationIds);
    }

    [HttpGet(StaticValues.DealerByMatchCriteriaInternalPath)]
    [ProducesResponseType(typeof(List<DealerResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<List<DealerResponse>> GetDealersByMatchCriteria(
        [FromQuery] string zipcode,
        [FromQuery] List<string> jobCategoryCodes,
        [FromQuery] bool isOnboardingComplete)
    {
        return await _dealerService.GetDealersByMatchCriteria(zipcode, jobCategoryCodes, isOnboardingComplete);
    }

    [HttpPut(StaticValues.TermsAndConditionsPath)]
    [Authorize(UserRoleEnum.Dealer)]
    [ProducesResponseType(typeof(DealerTermsAndConditionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<DealerTermsAndConditionsResponse> UpdateDealerTermsAndConditions(
        [FromBody] DealerTermsAndConditionsRequest dealerTermsAndConditionsRequest)
    {
        return await _dealerService.UpdateDealerTermsAndConditions(dealerTermsAndConditionsRequest);
    }
}
