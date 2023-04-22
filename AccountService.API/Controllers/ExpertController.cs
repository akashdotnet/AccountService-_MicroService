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
[Route(StaticValues.ExpertControllerRoutePrefix)]
public class ExpertController : ControllerBase
{
    private readonly IExpertService _expertService;
    private readonly IMapper _mapper;

    public ExpertController(IExpertService expertService, IMapper mapper)
    {
        _expertService = expertService;
        _mapper = mapper;
    }

    [HttpPut]
    [Authorize(UserRoleEnum.Expert)]
    [ProducesResponseType(typeof(ExpertResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ExpertResponse> UpdateExpert([FromBody] UpdateExpertRequest updateExpertRequest)
    {
        Expert expert = await _expertService.UpdateExpert(updateExpertRequest);
        return _mapper.Map<ExpertResponse>(expert);
    }

    [HttpGet]
    [Authorize(UserRoleEnum.Expert)]
    [ProducesResponseType(typeof(ExpertResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ExpertResponse> GetExpert()
    {
        return await _expertService.GetExpertWithProfilePhoto();
    }

    [HttpGet(StaticValues.ExpertByEmailInternalPath)]
    [ProducesResponseType(typeof(ExpertResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ExpertResponse> GetExpertByEmail(string email)
    {
        return await _expertService.GetExpertByEmail(email);
    }

    [HttpPost(StaticValues.ExpertProfilesByEmailInternalPath)]
    [ProducesResponseType(typeof(List<ExpertResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<List<ExpertResponse>> GetExpertProfiles(
        [FromBody] ExpertByEmailRequest expertByEmailRequest)
    {
        return await _expertService.GetExpertProfiles(expertByEmailRequest);
    }

    [HttpPost(StaticValues.ProfilePhotoUploadPath)]
    [Authorize(UserRoleEnum.Expert)]
    [ProducesResponseType(typeof(ProfilePhotoUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ProfilePhotoUploadResponse>
        UploadExpertProfilePhoto([FromForm] ProfilePhotoUploadRequest profilePhotoUploadRequest)
    {
        return await _expertService.UploadExpertProfilePhoto(profilePhotoUploadRequest);
    }
}
