using System;
using System.Linq;
using AccountService.API.Dto.PaymentServiceClient;
using AccountService.API.Dto.Request;
using AccountService.API.Dto.Response;
using AccountService.API.Dto.SalesForce;
using AccountService.API.Models;
using AccountService.API.Services;
using AccountService.API.Utils;
using AutoMapper;

namespace AccountService.API.Config;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CustomerRegistrationRequest, Account>();
        CreateMap<UserInformationResponse, Account>();
        CreateMap<NotificationPreferenceRequest, Customer>();
        CreateMap<Customer, NotificationPreferenceResponse>();
        CreateMap<UpdateCustomerRequest, Customer>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<CustomerAddress, CustomerAddressResponse>().ForMember(
                d => d.ZipCode,
                s => s.MapFrom(m => m.Address.ZipCode)
            ).ForMember(
                d => d.AddressValue,
                s => s.MapFrom(m => m.Address.AddressValue))
            .ForMember(
                d => d.City,
                s => s.MapFrom(m => m.Address.City))
            .ForMember(
                d => d.State,
                s => s.MapFrom(m => m.Address.State)
            )
            .ForMember(
                d => d.IsPrimaryAddress,
                s => s.MapFrom(m => m.Address.IsPrimaryAddress)
            );
        CreateMap<Customer, CustomerResponse>().ForMember(
            d => d.FirstName,
            s => s.MapFrom(m => m.Account.FirstName)
        ).ForMember(
            d => d.LastName,
            s => s.MapFrom(m => m.Account.LastName)
        ).ForMember(
            d => d.Email,
            s => s.MapFrom(m => m.Account.Email)
        ).ForMember(
            d => d.PhoneNumber,
            s => s.MapFrom(m => m.Account.PhoneNumber)
        ).ForMember(
            d => d.IsOnboardingComplete,
            s => s.MapFrom(m => m.Account.IsOnboardingComplete)
        );
        CreateMap<Customer, CustomerProfileByEmailResponse>().ForMember(
            d => d.FirstName,
            s => s.MapFrom(m => m.Account.FirstName)
        ).ForMember(
            d => d.LastName,
            s => s.MapFrom(m => m.Account.LastName)
        ).ForMember(
            d => d.Email,
            s => s.MapFrom(m => m.Account.Email)
        );
        CreateMap<Dealer, DealerResponse>().ForMember(
            d => d.FirstName,
            s => s.MapFrom(m => m.Account.FirstName)
        ).ForMember(
            d => d.LastName,
            s => s.MapFrom(m => m.Account.LastName)
        ).ForMember(
            d => d.Email,
            s => s.MapFrom(m => m.Account.Email)
        ).ForMember(
            d => d.PhoneNumber,
            s => s.MapFrom(m => m.Account.PhoneNumber)
        ).ForMember(
            d => d.IsOnboardingComplete,
            s => s.MapFrom(m => m.Account.IsOnboardingComplete)
        );
        CreateMap<UpdateDealerRequest, Dealer>();
        CreateMap<Business, BusinessResponse>()
            .ForMember(d => d.StartYear,
                s => s.MapFrom(m => YearUtils.GetStartYearOptionValue(m.StartYear)))
            .ForMember(d => d.ExperienceInYears,
                s => s.MapFrom(m => YearUtils.GetYearsOfExperienceFromStartYear(m.StartYear, DateTime.UtcNow.Year)));

        CreateMap<BusinessLocation, BusinessLocationResponse>()
            .ForMember(
                d => d.ServiceableCounties,
                s => s
                    .MapFrom(m =>
                        m.BusinessLocationServiceableCounties.ConvertAll(input => input.ServiceableCounty))
            );


        CreateMap<BusinessRequest, Business>();

        CreateMap<BusinessLocationRequest, BusinessLocation>()
            .ForMember(
                d => d.BusinessLocationServiceableCounties,
                s => s
                    .MapFrom(m => m.ServiceableCounties.Select(input => new BusinessLocationServiceableCounty
                        {ServiceableCounty = input}))
            ).ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<BusinessJobCategory, JobCategoryResponse>();
        CreateMap<ExpertSkill, SkillResponse>();
        CreateMap<ExpertLanguage, LanguageResponse>();
        CreateMap<BusinessBrand, BusinessBrandResponse>();
        CreateMap<BusinessBrandRequest, BusinessBrand>();
        CreateMap<Expert, ExpertResponse>()
            .ForMember(
                d => d.FirstName,
                s => s.MapFrom(m => m.Account.FirstName)
            )
            .ForMember(
                d => d.LastName,
                s => s.MapFrom(m => m.Account.LastName)
            ).ForMember(
                d => d.Email,
                s => s.MapFrom(m => m.Account.Email)
            ).ForMember(
                d => d.PhoneNumber,
                s => s.MapFrom(m => m.Account.PhoneNumber)
            ).ForMember(
                d => d.IsOnboardingComplete,
                s => s.MapFrom(m => m.Account.IsOnboardingComplete)
            ).ForMember(
                d => d.ExperienceInYears,
                s => s.MapFrom(m => ExpertService.GetYearsOfExperienceFromJoiningYear(m.JoiningYear))
            );
        CreateMap<Dealer, DealerProfileResponse>()
            .ForMember(d => d.Email,
                s => s.MapFrom(m => m.Account.Email));
        CreateMap<Business, BusinessProfileResponse>();
        CreateMap<BusinessLocation, BaseBusinessLocationResponse>();
        CreateMap<Dealer, DealerLocationProfileResponse>()
            .ForMember(
                d => d.Email,
                s => s.MapFrom(m => m.Account.Email)
            ).ForMember(
                d => d.FirstName,
                s => s.MapFrom(m => m.Account.FirstName)
            ).ForMember(
                d => d.LastName,
                s => s.MapFrom(m => m.Account.LastName)
            );
        CreateMap<Business, BusinessProfileDetailsResponse>()
            .ForMember(d => d.ExperienceInYears,
                s => s.MapFrom(
                    m => YearUtils.GetYearsOfExperienceFromStartYear(m.StartYear, DateTime.UtcNow.Year)));
        CreateMap<DeliveryInstructionsRequest, CustomerDeliveryInstruction>()
            .ForMember(d => d.Customer, s => s.Ignore());
        CreateMap<CustomerDeliveryInstruction, DeliveryInstructionsResponse>();
        CreateMap<CustomerSuggestedDealer, CustomerSuggestedDealerResponse>();
        CreateMap<CustomerSuggestedDealerRequest, CustomerSuggestedDealer>()
            .ForMember(d => d.Customer, s => s.Ignore());
        CreateMap<CustomerAddressRequest, Address>();
        CreateMap<DealerAddressRequest, Address>();
        CreateMap<Address, AddressResponse>();
        CreateMap<Customer, CreateCustomerForPaymentRequest>()
            .ForMember(d => d.Email,
                s => s.MapFrom(
                    m => m.Account.Email)
            )
            .ForMember(d => d.Phone,
                s => s.MapFrom(
                    m => m.Account.PhoneNumber))
            .ForMember(d => d.Name,
                s => s.MapFrom(
                    m => $"{m.Account.FirstName} {m.Account.LastName}".Trim()));
        CreateMap<UpdateCustomerRequest, Account>()
            .ForAllMembers(opts => opts.Condition(
                (src, dest, srcMember) =>
                {
                    if (srcMember is string srcMemberAsString)
                    {
                        return !string.IsNullOrEmpty(srcMemberAsString);
                    }

                    return srcMember != null;
                }
            ));
        CreateMap<Dealer, DealerTermsAndConditionsResponse>();
        CreateMap<DealerTermsAndConditionsRequest, Dealer>();
    }
}
