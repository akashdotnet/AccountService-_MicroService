using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountService.API.Clients.Interfaces;
using AccountService.API.Constants;
using AccountService.API.Dto.CatalogServiceClient;
using AccountService.API.Dto.JobServiceClient;
using AccountService.API.Dto.Request;
using AccountService.API.Dto.Response;
using AccountService.API.Models;
using AccountService.API.Repositories.Interfaces;
using AccountService.API.Services.Interfaces;
using AccountService.API.Utils;
using AutoMapper;
using PodCommonsLibrary.Core.Exceptions;

namespace AccountService.API.Services;

public class BusinessService : IBusinessService
{
    private readonly IAccountService _accountService;
    private readonly IBusinessRepository _businessRepository;
    private readonly ICatalogServiceClient _catalogServiceClient;
    private readonly IDealerRepository _dealerRepository;
    private readonly IJobServiceClient _jobServiceClient;
    private readonly IMapper _mapper;

    public BusinessService(IBusinessRepository businessRepository,
        ICatalogServiceClient catalogServiceClient, IJobServiceClient jobServiceClient, IMapper mapper,
        IAccountService accountService,
        IDealerRepository dealerRepository)
    {
        _businessRepository = businessRepository;
        _accountService = accountService;
        _catalogServiceClient = catalogServiceClient;
        _jobServiceClient = jobServiceClient;
        _dealerRepository = dealerRepository;
        _mapper = mapper;
    }

    public async Task<BusinessLogoUploadResponse> UploadBusinessLogo(
        BusinessLogoUploadRequest businessLogoUploadRequest)
    {
        int accountId = await _accountService.GetAccountId();
        Dealer? dealer = await _dealerRepository.GetDealerAndBusinessByAccountId(accountId);
        Business business = dealer?.Business ?? throw new BusinessRuleViolationException(
            StaticValues.BusinessNotFound,
            StaticValues.ErrorBusinessNotFoundForDealer);
        int dealerId = dealer.Id;
        string logoFileName =
            StaticValues.GetBusinessLogoFileName(businessLogoUploadRequest.BusinessLogo, dealerId);
        BlobResponse blobResponse = await _catalogServiceClient.UploadFile(
            businessLogoUploadRequest.BusinessLogo, logoFileName, StaticValues.DealersContainerName,
            business.LogoBlobId);
        business.LogoBlobId = blobResponse.BlobId;
        await _businessRepository.SaveBusiness(business);
        return new BusinessLogoUploadResponse
        {
            BusinessLogoUrl = blobResponse.BlobUrl
        };
    }

    public async Task<Business> CreateOrUpdateBusiness(Business? existingBusiness,
        BusinessRequest businessRequest)
    {
        Business newBusiness = existingBusiness ?? new Business();
        newBusiness.About = businessRequest.About ?? newBusiness.About;
        newBusiness.WebsiteUrl = businessRequest.WebsiteUrl ?? newBusiness.WebsiteUrl;
        newBusiness.PhoneNumber = businessRequest.PhoneNumber ?? newBusiness.PhoneNumber;
        newBusiness.PreferredCommunicationsEmail =
            businessRequest.PreferredCommunicationsEmail ?? newBusiness.PreferredCommunicationsEmail;
        newBusiness.StartYear = businessRequest.StartYear != null
            ? YearUtils.GetStartYearAsInteger(businessRequest.StartYear, DateTime.UtcNow.Year)
            : newBusiness.StartYear;
        newBusiness.PoolCount = businessRequest.PoolCount ?? newBusiness.PoolCount;
        if (businessRequest.Name != null)
        {
            newBusiness.Name = businessRequest.Name;
        }

        if (businessRequest.Brands != null)
        {
            newBusiness.Brands = await CreateOrUpdateBrands(newBusiness.Brands, businessRequest.Brands);
        }

        if (businessRequest.Locations != null)
        {
            newBusiness.Locations = await CreateOrUpdateBusinessLocation(newBusiness.Locations,
                businessRequest.Locations, newBusiness.Id);
        }

        if (businessRequest.Categories != null)
        {
            newBusiness.Categories = JobCategoryService.CreateOrUpdateJobCategories(newBusiness.Categories,
                businessRequest.Categories);
        }

        return newBusiness;
    }

    private async Task<List<BusinessBrand>> CreateOrUpdateBrands(List<BusinessBrand> existingBusinessBrands,
        BusinessBrandRequest? newBrandCodes)
    {
        if (newBrandCodes == null)
        {
            return existingBusinessBrands;
        }

        List<BrandResponse> brandResponses = await _catalogServiceClient.GetBrands();

        IEnumerable<BusinessBrand> businessBrands = new HashSet<string>(newBrandCodes.Codes).Select(
            incomingBrandCode =>
            {
                ValidationService.ValidateBrandCode(incomingBrandCode, brandResponses);

                string? othersData = newBrandCodes.Codes?
                                         .FirstOrDefault(x => x.Contains(StaticValues.OthersCode)) != null
                                     && incomingBrandCode == StaticValues.OthersCode
                    ? newBrandCodes.Others
                    : null;
                //use the same object if the brand code is already mapped to business
                BusinessBrand businessBrandObj = existingBusinessBrands.Find(existingBusinessBrand =>
                                                     existingBusinessBrand.Code == incomingBrandCode)
                                                 ?? new BusinessBrand
                                                 {
                                                     Code = incomingBrandCode
                                                 };
                businessBrandObj.Others = othersData;
                return businessBrandObj;
            }
        );

        return businessBrands.ToList();
    }

    private async Task<List<BusinessLocation>> CreateOrUpdateBusinessLocation(
        List<BusinessLocation> existingBusinessLocations,
        List<BusinessLocationRequest> businessLocationRequests, int businessId)
    {
        List<StateResponse> stateResponses = await _catalogServiceClient.GetStates(true);

        // business location ids to be deleted = existing business location ids -  business location ids in request
        List<int> businessLocationIdsToBeDeleted = existingBusinessLocations
            .FindAll(businessLocation => !businessLocationRequests.Any(businessLocationRequest =>
                businessLocationRequest.Id.Equals(businessLocation.Id)))
            .ConvertAll(businessLocation => businessLocation.Id);

        // check if there is an existing work order for a location before deleting them
        if (businessLocationIdsToBeDeleted.Any())
        {
            List<WorkOrderResponse> workOrders =
                await _jobServiceClient.GetWorkOrdersByBusinessLocationIds(businessLocationIdsToBeDeleted);
            if (workOrders.Any())
            {
                throw new BusinessRuleViolationException(
                    StaticValues.ExistingWorkOrder,
                    StaticValues.ExistingWorkOrderErrorMessage(workOrders.ConvertAll(workOrder =>
                        workOrder.BusinessLocationId))
                );
            }
        }

        IEnumerable<BusinessLocation> businessLocations = businessLocationRequests.Select(
            businessLocationRequest =>
            {
                ValidationService.ValidateBusinessLocation(businessLocationRequest, stateResponses);
                BusinessLocation? updatedBusinessLocation;
                // id=null => location to be created 
                if (businessLocationRequest.Id is null or 0)
                {
                    // the below mapper takes care of mapping the request to model with same keys like Address, OfficeName
                    updatedBusinessLocation = _mapper.Map<BusinessLocation>(businessLocationRequest);
                }
                // id=!null => location to be updated 
                else
                {
                    // verify if the id of the location is valid
                    updatedBusinessLocation =
                        existingBusinessLocations.Find(existingBusinessLocation =>
                            existingBusinessLocation.Id == (int) businessLocationRequest.Id);

                    if (updatedBusinessLocation == null)
                    {
                        throw new BusinessRuleViolationException(StaticValues.LocationNotFound,
                            StaticValues.ErrorLocationNotFound((int) businessLocationRequest.Id, businessId));
                    }

                    //merge the businessLocationRequest and updatedBusinessLocation data to updatedBusinessLocation (only the non null values are copied)
                    _mapper.Map(businessLocationRequest, updatedBusinessLocation);
                }

                if (businessLocationRequest.ServiceableCounties != null)
                {
                    updatedBusinessLocation.BusinessLocationServiceableCounties =
                        CreateOrUpdateServiceableCounties(
                            businessLocationRequest.ServiceableCounties,
                            updatedBusinessLocation.BusinessLocationServiceableCounties);
                }

                return updatedBusinessLocation;
            });
        return businessLocations.ToList();
    }

    private List<BusinessLocationServiceableCounty> CreateOrUpdateServiceableCounties(
        List<string> newServiceableCounties,
        List<BusinessLocationServiceableCounty> existingBusinessLocationServiceableCounties)
    {
        IEnumerable<BusinessLocationServiceableCounty> businessLocationServiceableCounties =
            new HashSet<string>(newServiceableCounties).Select(serviceableCounty =>
            {
                //use the same object if the service zip code is already mapped to business location
                return existingBusinessLocationServiceableCounties?.Find(
                    locationServiceZipCode =>
                        locationServiceZipCode.ServiceableCounty == serviceableCounty
                ) ?? new BusinessLocationServiceableCounty
                {
                    ServiceableCounty = serviceableCounty
                };
            });
        return businessLocationServiceableCounties.ToList();
    }
}
