using System.Threading.Tasks;
using AccountService.API.Dto;
using AccountService.API.Dto.Request;

namespace AccountService.API.Services.Interfaces;

public interface IOtpService
{
    Task<Session> GenerateOtp(Session sessionRequest);
    Task<Session> ValidateOtp(ValidateOtpRequest validateOtpRequest);
}
