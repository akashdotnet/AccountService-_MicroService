using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AccountService.API.Clients.Interfaces;
using AccountService.API.Config;
using AccountService.API.Constants;
using AccountService.API.Dto.CatalogServiceClient;
using AccountService.API.Dto.JobServiceClient;
using AccountService.API.Dto.Request;
using AccountService.API.Dto.Response;
using AccountService.API.Enums;
using AccountService.API.Models;
using AccountService.API.Repositories.Interfaces;
using AccountService.API.Services;
using AccountService.API.Services.Interfaces;
using AutoFixture;
using AutoFixture.DataAnnotations;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Moq;
using PodCommonsLibrary.Core.Exceptions;
using Xunit;

namespace AccountService.API.Tests.Services;

public class BusinessServiceTests
{
    private readonly Mock<IAccountService> _accountServiceMock;
    private readonly Mock<IBusinessRepository> _businessRepositoryMock;
    private readonly BusinessService _businessService;
    private readonly Mock<ICatalogServiceClient> _catalogServiceClientMock;
    private readonly Mock<IDealerRepository> _dealerRepositoryMock;
    private readonly IFixture _fixture;
    private readonly Mock<IJobServiceClient> _jobServiceClientMock;

    public BusinessServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Customizations.Add(new RegularExpressionGenerator());
        _fixture.Customizations.Add(new RegularExpressionAttributeRelay());
        _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _accountServiceMock = new Mock<IAccountService>();
        _businessRepositoryMock = new Mock<IBusinessRepository>();
        MapperConfiguration configuration = new(cfg => cfg.AddProfile(new MappingProfile()));
        IMapper mapper = new Mapper(configuration);
        _dealerRepositoryMock = new Mock<IDealerRepository>();
        _catalogServiceClientMock = new Mock<ICatalogServiceClient>();
        _accountServiceMock = new Mock<IAccountService>();
        _jobServiceClientMock = new Mock<IJobServiceClient>();

        _businessService = new BusinessService(_businessRepositoryMock.Object,
            _catalogServiceClientMock.Object, _jobServiceClientMock.Object, mapper, _accountServiceMock.Object,
            _dealerRepositoryMock.Object);
    }

    [Fact(DisplayName =
        "BusinessService: UploadBusinessLogo - Should successfully upload business logo and store reference blob id.")]
    public async Task UploadBusinessLogo_Success()
    {
        Dealer dealerMock = _fixture.Create<Dealer>();

        IFormFile file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("dummy image")), 0, 0, "Data",
            "image.png");

        BusinessLogoUploadRequest businessLogoUploadRequestMock = new()
        {
            BusinessLogo = file
        };

        BlobResponse blobResponseMock = _fixture.Create<BlobResponse>();

        _accountServiceMock.Setup(m => m.GetAccountId(null)).ReturnsAsync(1);
        _dealerRepositoryMock.Setup(dealerRepository => dealerRepository
                .GetDealerAndBusinessByAccountId(1))
            .ReturnsAsync(dealerMock);
        _catalogServiceClientMock.Setup(m => m.UploadFile(businessLogoUploadRequestMock.BusinessLogo,
                $"{dealerMock.Id}/Business_Logo.png", "dealers", dealerMock.Business.LogoBlobId))
            .ReturnsAsync(blobResponseMock);

        BusinessLogoUploadResponse result =
            await _businessService.UploadBusinessLogo(businessLogoUploadRequestMock);
        Assert.Equal(blobResponseMock.BlobUrl, result.BusinessLogoUrl);

        _accountServiceMock.VerifyAll();
        _dealerRepositoryMock.VerifyAll();
        _catalogServiceClientMock.VerifyAll();
    }

    [Fact(DisplayName =
        "BusinessService: UploadBusinessLogo - Should throw BusinessRuleViolationException with type BusinessNotFound if the dealer if the business is not created for dealer.")]
    public async Task UploadBusinessLogo_BusinessNotFound()
    {
        Dealer dealerMock = _fixture.Build<Dealer>()
            .Without(x => x.Business)
            .Create();

        IFormFile file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("dummy image")), 0, 0, "Data",
            "image.png");

        BusinessLogoUploadRequest businessLogoUploadRequestMock = new()
        {
            BusinessLogo = file
        };

        _accountServiceMock.Setup(m => m.GetAccountId(null)).ReturnsAsync(1);
        _dealerRepositoryMock.Setup(dealerRepository => dealerRepository
                .GetDealerAndBusinessByAccountId(1))
            .ReturnsAsync((Dealer?) null);

        BusinessRuleViolationException businessRuleViolationException =
            await Assert.ThrowsAsync<BusinessRuleViolationException>(
                async () => await _businessService.UploadBusinessLogo(businessLogoUploadRequestMock));

        Assert.Equal(StaticValues.BusinessNotFound, businessRuleViolationException.ErrorResponseType);
        Assert.Equal("Unable to find business for the logged in user", businessRuleViolationException.Message);

        _accountServiceMock.VerifyAll();
        _dealerRepositoryMock.VerifyAll();
    }

    [Fact(DisplayName =
        "BusinessService: CreateBusiness - Should create an new business object based on business request if the business doesnt exist previously.")]
    public async Task CreateBusiness()
    {
        _catalogServiceClientMock.Setup(m => m.GetBrands())
            .ReturnsAsync(GetBrandResponsesMock());
        _catalogServiceClientMock.Setup(m => m.GetStates(true))
            .ReturnsAsync(GetStateResponseMock());
        _jobServiceClientMock.Setup(x => x.GetWorkOrdersByBusinessLocationIds(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<WorkOrderResponse>());

        BusinessRequest businessRequestMock = GetBusinessRequest();

        Business result =
            await _businessService.CreateOrUpdateBusiness(null, businessRequestMock);

        _dealerRepositoryMock.VerifyAll();
        _catalogServiceClientMock.VerifyAll();

        Assert.Single(result.Brands);
        Assert.Equal("valid_brand_code", result.Brands[0].Code);
        Assert.Single(result.Locations);
        Assert.Equal(businessRequestMock.Locations?[0].Address.City, result.Locations[0].Address.City);
        Assert.Equal(businessRequestMock.Locations?[0].Address.State, result.Locations[0].Address.State);
        Assert.Equal(businessRequestMock.Locations?[0].Address.AddressValue, result.Locations[0].Address.AddressValue);
        Assert.Single(result.Locations[0].BusinessLocationServiceableCounties);
        Assert.Equal(businessRequestMock.Locations?[0].ServiceableCounties?[0],
            result.Locations[0].BusinessLocationServiceableCounties[0].ServiceableCounty);
    }

    [Fact(DisplayName =
        "BusinessService: UpdateBusiness - Should throw BusinessRuleViolationException with type LocationNotFound.")]
    public async Task UpdateBusiness_LocationNotFound()
    {
        _catalogServiceClientMock.Setup(m => m.GetBrands())
            .ReturnsAsync(GetBrandResponsesMock());
        _catalogServiceClientMock.Setup(m => m.GetStates(true))
            .ReturnsAsync(GetStateResponseMock());
        _jobServiceClientMock.Setup(x => x.GetWorkOrdersByBusinessLocationIds(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<WorkOrderResponse>());
        const int invalidLocationId = 6;
        BusinessRequest businessRequestMock = GetBusinessRequest(invalidLocationId);
        BusinessLocation businessLocationMock = GetBusinessLocationMock();
        Business businessMock = _fixture.Build<Business>()
            .With(x => x.Locations, new List<BusinessLocation> {businessLocationMock})
            .Create();
        BusinessRuleViolationException businessRuleViolationException =
            await Assert.ThrowsAsync<BusinessRuleViolationException>(
                async () => await _businessService.CreateOrUpdateBusiness(businessMock,
                    businessRequestMock));


        _dealerRepositoryMock.VerifyAll();
        _catalogServiceClientMock.VerifyAll();

        Assert.Equal(businessRuleViolationException.ErrorResponseType, StaticValues.LocationNotFound);
        Assert.Equal($"Unable to find location with id: {invalidLocationId} the business: {businessMock.Id}",
            businessRuleViolationException.Message);
    }

    [Fact(DisplayName =
        "BusinessService: UpdateBusiness - Should throw BusinessRuleViolationException with type ExistingWorkOrder while attempting to delete a business location with existing work order.")]
    public async Task UpdateBusiness_ExistingWorkOrder_ThrowsBusinessRuleViolationException()
    {
        const int existingBusinessLocationId = 2;

        _catalogServiceClientMock.Setup(m => m.GetBrands())
            .ReturnsAsync(GetBrandResponsesMock());
        _catalogServiceClientMock.Setup(m => m.GetStates(true))
            .ReturnsAsync(GetStateResponseMock());
        WorkOrderResponse workOrderMock = _fixture.Build<WorkOrderResponse>()
            .With(x => x.BusinessLocationId, existingBusinessLocationId)
            .Create();
        _jobServiceClientMock.Setup(x => x.GetWorkOrdersByBusinessLocationIds(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<WorkOrderResponse> {workOrderMock});
        BusinessRequest businessRequestMock = GetBusinessRequest();
        BusinessLocation businessLocationMock = GetBusinessLocationMock(existingBusinessLocationId);
        Business businessMock = _fixture.Build<Business>()
            .With(x => x.Locations, new List<BusinessLocation> {businessLocationMock})
            .Create();

        BusinessRuleViolationException businessRuleViolationException =
            await Assert.ThrowsAsync<BusinessRuleViolationException>(
                async () => await _businessService.CreateOrUpdateBusiness(businessMock,
                    businessRequestMock));

        _dealerRepositoryMock.VerifyAll();
        _catalogServiceClientMock.VerifyAll();

        Assert.Equal(StaticValues.ExistingWorkOrder, businessRuleViolationException.ErrorResponseType);
        Assert.Equal(
            $"The following business locations with ids: {existingBusinessLocationId} cannot be deleted as there are existing work orders associated with them.",
            businessRuleViolationException.Message);
    }

    [Fact(DisplayName =
        "BusinessService: UpdateBusiness - Should update the existing business based on business request.")]
    public async Task UpdateBusiness()
    {
        _catalogServiceClientMock.Setup(m => m.GetBrands())
            .ReturnsAsync(GetBrandResponsesMock());
        _catalogServiceClientMock.Setup(m => m.GetStates(true))
            .ReturnsAsync(GetStateResponseMock());
        _jobServiceClientMock.Setup(x => x.GetWorkOrdersByBusinessLocationIds(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<WorkOrderResponse>());
        Business businessMock = _fixture.Create<Business>();
        BusinessRequest businessRequestMock = GetBusinessRequest();

        Business result =
            await _businessService.CreateOrUpdateBusiness(businessMock, businessRequestMock);

        _dealerRepositoryMock.VerifyAll();
        _catalogServiceClientMock.VerifyAll();

        Assert.Single(result.Brands);
        Assert.Equal("valid_brand_code", result.Brands[0].Code);
        Assert.Single(result.Locations);
        Assert.Equal(businessRequestMock.Locations?[0].Address.City, result.Locations[0].Address.City);
        Assert.Equal(businessRequestMock.Locations?[0].Address.State, result.Locations[0].Address.State);
        Assert.Equal(businessRequestMock.Locations?[0].Address.AddressValue, result.Locations[0].Address.AddressValue);
        Assert.Single(result.Locations[0].BusinessLocationServiceableCounties);
        Assert.Equal(businessRequestMock.Locations?[0].ServiceableCounties?[0],
            result.Locations[0].BusinessLocationServiceableCounties[0].ServiceableCounty);
    }

    private static BusinessRequest GetBusinessRequest(int? locationId = null)
    {
        return new BusinessRequest
        {
            Name = "Business Name",
            Locations = new List<BusinessLocationRequest>
            {
                new()
                {
                    Id = locationId,
                    OfficeName = "Location Name",
                    Address = new DealerAddressRequest
                    {
                        AddressValue = "Address Value",
                        City = "City",
                        State = "Alabama",
                        ZipCode = "36006",
                        IsPrimaryAddress = true
                    },
                    ServiceableCounties = new List<string>
                    {
                        "Autauga County"
                    }
                }
            },
            Brands = new BusinessBrandRequest
            {
                Codes = new List<string> {"valid_brand_code"}
            },
            Categories = new List<JobCategoriesRequest>
            {
                new()
                {
                    Type = JobCategoryTypeEnum.Repair,
                    Codes = new List<string> {"pump_repair"}
                },
                new()
                {
                    Type = JobCategoryTypeEnum.Service,
                    Codes = new List<string> {"free_assessment"}
                }
            }
        };
    }

    private static List<BrandResponse> GetBrandResponsesMock()
    {
        return new List<BrandResponse>
        {
            new()
            {
                Code = "valid_brand_code"
            }
        };
    }

    private static BusinessLocation GetBusinessLocationMock(int? locationId = null)
    {
        return new BusinessLocation
        {
            Id = locationId ?? 1,
            Address = new Address
            {
                AddressValue = "Address Value",
                City = "City",
                State = "Alabama",
                ZipCode = "36006",
                IsPrimaryAddress = true
            },
            OfficeName = "Location name",
            BusinessLocationServiceableCounties = new List<BusinessLocationServiceableCounty>
            {
                new()
                {
                    ServiceableCounty = "St James"
                }
            }
        };
    }

    private static List<StateResponse> GetStateResponseMock()
    {
        return new List<StateResponse>
        {
            new()
            {
                Name = "Alabama",
                Counties = new List<CountyResponse>
                {
                    new()
                    {
                        Name = "Autauga County",
                        ZipCodes = new List<string> {"36006"}
                    }
                }
            }
        };
    }
}
