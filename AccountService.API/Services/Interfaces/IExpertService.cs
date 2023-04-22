using System.Collections.Generic;
using System.Threading.Tasks;
using AccountService.API.Dto.Request;
using AccountService.API.Dto.Response;
using AccountService.API.Dto.SalesForce;
using AccountService.API.Models;

namespace AccountService.API.Services.Interfaces;

public interface IExpertService
{
    Task<ExpertResponse> CreateExpert(UserInformationResponse userInformation);
    Task<List<ExpertResponse>> GetAllExpertProfiles();
    Task<Expert> UpdateExpert(UpdateExpertRequest updateExpertRequest);
    Task<ExpertResponse> GetExpertWithProfilePhoto();
    Task<ExpertResponse> GetExpertByEmail(string email);
    Task<List<ExpertResponse>> GetExpertProfiles(ExpertByEmailRequest expertByEmailRequest);
    Task<ProfilePhotoUploadResponse> UploadExpertProfilePhoto(ProfilePhotoUploadRequest profilePhotoUploadRequest);
}
