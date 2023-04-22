using System.Threading.Tasks;
using AccountService.API.Exceptions;
using Audit.WebApi;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PodCommonsLibrary.Core.Dto;

namespace AccountService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;

    public HealthController(HealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    [HttpGet]
    [AuditIgnore]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<string> Get()
    {
        HealthReport healthReport = await _healthCheckService.CheckHealthAsync();
        if (healthReport.Status != HealthStatus.Healthy)
        {
            throw new ServiceUnavailableException(healthReport.Status.ToString());
        }

        return healthReport.Status.ToString();
    }
}
