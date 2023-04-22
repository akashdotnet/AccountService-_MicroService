using System.Threading.Tasks;
using AccountService.API.Dto.Request;
using AccountService.API.Dto.Response;
using AccountService.API.Models;

namespace AccountService.API.Services.Interfaces;

public interface IBusinessService
{
    Task<BusinessLogoUploadResponse> UploadBusinessLogo(
        BusinessLogoUploadRequest businessLogoUploadRequest);

    Task<Business> CreateOrUpdateBusiness(Business existingBusiness,
        BusinessRequest dealerBusinessRequest);
}
