using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AccountService.API.Clients.Interfaces;
using AccountService.API.Constants;
using AccountService.API.Dto.CatalogServiceClient;
using AccountService.API.Dto.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using PodCommonsLibrary.Core.Exceptions;
using PodCommonsLibrary.Core.Utils;

namespace AccountService.API.Clients;

public class CatalogServiceClient : ICatalogServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly HttpClientUtility _httpClientUtility;

    public CatalogServiceClient(HttpClient httpClient, HttpClientUtility httpClientUtility)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        ValidateBaseAddress(_httpClient.BaseAddress);
        _httpClientUtility = httpClientUtility;
    }

    public async Task<List<BrandResponse>> GetBrands()
    {
        Uri uriPath = new(_httpClient.BaseAddress, StaticValues.BrandsPath);
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(uriPath);
        return await _httpClientUtility.PrepareResponse<List<BrandResponse>>(httpResponseMessage, uriPath);
    }

    public async Task<BlobResponse> UploadFile(IFormFile iFormFile, string fileName, string containerName, int? blobId)
    {
        Uri uriPath = new(_httpClient.BaseAddress, StaticValues.UploadFilePath);
        StreamContent streamContent = new(iFormFile.OpenReadStream());
        MultipartFormDataContent content = new()
        {
            {new StringContent(fileName), nameof(fileName)},
            {
                new StringContent(containerName),
                nameof(containerName)
            },
            {new StringContent(blobId?.ToString() ?? ""), nameof(blobId)}
        };
        content.Add(streamContent, "file", iFormFile.FileName);
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsync(uriPath, content);
        return await _httpClientUtility.PrepareResponse<BlobResponse>(httpResponseMessage, uriPath);
    }

    public async Task<BlobResponse> GetBlobUrl(int blobId)
    {
        Uri uriPath = new(_httpClient.BaseAddress, StaticValues.GetBlobByBlobIdUrlPath(blobId));
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(uriPath);
        return await _httpClientUtility.PrepareResponse<BlobResponse>(httpResponseMessage, uriPath);
    }

    public async Task<List<StateResponse>> GetStates(bool includeCounties)
    {
        Dictionary<string, string> param = new()
        {
            {"includeCounties", includeCounties.ToString()}
        };

        string baseUrl = $"{_httpClient.BaseAddress}{StaticValues.StatesPath}";
        Uri uriPath = new(QueryHelpers.AddQueryString(baseUrl, param!));
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(uriPath);
        return await _httpClientUtility.PrepareResponse<List<StateResponse>>(httpResponseMessage, uriPath);
    }

    public async Task<StateByZipCodeResponse> GetStateAndCountyByZipCode(string zipCode)
    {
        Dictionary<string, string> param = new()
        {
            {"zipCode", zipCode}
        };

        string baseUrl = $"{_httpClient.BaseAddress}{StaticValues.CountyByZipCodePath}";
        Uri uriPath = new(QueryHelpers.AddQueryString(baseUrl, param!));
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(uriPath);
        return await _httpClientUtility.PrepareResponse<StateByZipCodeResponse>(httpResponseMessage, uriPath);
    }

    public async Task<PoolDetailLookups> GetPoolDetailLookups()
    {
        Uri uriPath = new(_httpClient.BaseAddress, StaticValues.PoolDetailLookupsPath);
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(uriPath);
        return await _httpClientUtility.PrepareResponse<PoolDetailLookups>(httpResponseMessage, uriPath);
    }

    public async Task<List<JobCategoryResponse>> GetJobCategories()
    {
        Uri uriPath = new(_httpClient.BaseAddress, StaticValues.JobCategoriesPath);
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(uriPath);
        return await _httpClientUtility.PrepareResponse<List<JobCategoryResponse>>(httpResponseMessage, uriPath);
    }

    public async Task<List<LanguageResponse>> GetLanguages()
    {
        Uri uriPath = new(_httpClient.BaseAddress, StaticValues.LanguagesPath);
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(uriPath);
        return await _httpClientUtility.PrepareResponse<List<LanguageResponse>>(httpResponseMessage, uriPath);
    }

    public async Task<List<SkillResponse>> GetSkills()
    {
        Uri uriPath = new(_httpClient.BaseAddress, StaticValues.SkillsPath);
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(uriPath);
        return await _httpClientUtility.PrepareResponse<List<SkillResponse>>(httpResponseMessage, uriPath);
    }

    public async Task DeleteBlobById(int blobId)
    {
        Uri uriPath = new(_httpClient.BaseAddress, $"{StaticValues.BlobsPath}/{blobId}");
        HttpResponseMessage httpResponseMessage = await _httpClient.DeleteAsync(uriPath);
        await _httpClientUtility.PrepareResponse(httpResponseMessage, uriPath);
    }

    public async Task<List<BlobResponse>> GetBlobs(List<int> blobIds)
    {
        IEnumerable<KeyValuePair<string, string>> queryParams = blobIds.ConvertAll(blobId =>
            new KeyValuePair<string, string>("blobIds", blobId.ToString()));
        string baseUrl = $"{_httpClient.BaseAddress}{StaticValues.GetBlobsByBlobIdsUrlPath()}";
        Uri uriPath = new(QueryHelpers.AddQueryString(baseUrl, queryParams!));
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(uriPath);
        return await _httpClientUtility.PrepareResponse<List<BlobResponse>>(httpResponseMessage, uriPath);
    }

    private static void ValidateBaseAddress(Uri? baseAddress)
    {
        if (baseAddress == null)
        {
            throw new NotFoundException(StaticValues.CatalogServiceBaseAddressNotFound,
                StaticValues.ErrorCatalogServiceBaseAddressNotFound);
        }
    }
}
