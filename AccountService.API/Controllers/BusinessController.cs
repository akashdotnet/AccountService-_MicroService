using System.Threading.Tasks;
using AccountService.API.Constants;
using AccountService.API.Dto.Request;
using AccountService.API.Dto.Response;
using AccountService.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PodCommonsLibrary.Core.Annotations;
using PodCommonsLibrary.Core.Dto;
using PodCommonsLibrary.Core.Enums;

namespace AccountService.API.Controllers;

[ApiController]
[Authorize(UserRoleEnum.Dealer)]
[Route(StaticValues.BusinessControllerRoutePrefix)]
public class BusinessController : ControllerBase
{
    private readonly IBusinessService _businessService;

    public BusinessController(IBusinessService businessService)
    {
        _businessService = businessService;
    }

    [HttpPost(StaticValues.BusinessLogoUploadPath)]
    [ProducesResponseType(typeof(BusinessLogoUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<BusinessLogoUploadResponse>
        UploadBusinessLogo([FromForm] BusinessLogoUploadRequest businessLogoUploadRequest)
    {
        return await _businessService.UploadBusinessLogo(businessLogoUploadRequest);
    }
}
