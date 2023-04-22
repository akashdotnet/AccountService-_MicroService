using System.Threading.Tasks;
using AccountService.API.Dto;
using AccountService.API.Dto.Request;
using AccountService.API.Dto.Response;
using Microsoft.AspNetCore.Identity;
using PodCommonsLibrary.Core.Enums;

namespace AccountService.API.Services.Interfaces;

public interface IAuthService
{
    Task<IdentityResult?> CreateIdentityUser(BaseUserRegistrationRequest baseUserRegistrationRequest,
        UserRoleEnum role);

    Task<LoginResponse> GenerateToken(LoginRequest loginRequest);
    Task<LoginResponse> RefreshToken(RefreshTokenRequest refreshTokenRequest);
    Task<Session> GenerateResetToken(CustomerResetPasswordRequest customerResetPasswordRequest);
    Task<IdentityResult> SetNewPassword(CustomerSetNewPassword customerSetNewPassword);

    Task ChangeCustomerPassword(ChangeCustomerPasswordRequest changeCustomerPasswordRequest,
        string customerEmail);

    Task CheckIfUserExists(string email);
    Task LogoutCustomer(RefreshTokenRequest refreshTokenRequest);
}
