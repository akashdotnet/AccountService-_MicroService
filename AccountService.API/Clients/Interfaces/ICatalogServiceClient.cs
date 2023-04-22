using System.Collections.Generic;
using System.Threading.Tasks;
using AccountService.API.Dto.CatalogServiceClient;
using AccountService.API.Dto.Response;
using Microsoft.AspNetCore.Http;

namespace AccountService.API.Clients.Interfaces;

public interface ICatalogServiceClient
{
    Task<List<BrandResponse>> GetBrands();
    Task<BlobResponse> UploadFile(IFormFile iFormFile, string fileName, string containerName, int? blobId);
    Task<BlobResponse> GetBlobUrl(int blobId);
    Task<List<StateResponse>> GetStates(bool includeCounties = false);
    Task<PoolDetailLookups> GetPoolDetailLookups();
    Task<List<JobCategoryResponse>> GetJobCategories();
    Task<List<LanguageResponse>> GetLanguages();
    Task<List<SkillResponse>> GetSkills();
    Task<StateByZipCodeResponse> GetStateAndCountyByZipCode(string zipCode);
    Task DeleteBlobById(int blobId);
    Task<List<BlobResponse>> GetBlobs(List<int> BlobIds);
}
