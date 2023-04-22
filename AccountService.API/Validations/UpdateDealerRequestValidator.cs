using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AccountService.API.Clients.Interfaces;
using AccountService.API.Constants;
using AccountService.API.Dto.Request;
using AccountService.API.Dto.Response;
using AccountService.API.Enums;
using FluentValidation;

namespace AccountService.API.Validations;

public class UpdateDealerRequestValidator : AbstractValidator<UpdateDealerRequest>
{
    private readonly ICatalogServiceClient _catalogServiceClient;

    public UpdateDealerRequestValidator(ICatalogServiceClient catalogServiceClient)
    {
        _catalogServiceClient = catalogServiceClient ?? throw new ArgumentNullException(nameof(catalogServiceClient));
        List<JobCategoryResponse> jobCategoryResponses = new();

        RuleFor(updateDealerRequest => updateDealerRequest.Business)
            .NotNull();
        When(updateDealerRequest => updateDealerRequest.OnboardingStep == DealerOnboardingStepEnum.AboutBusiness, () =>
        {
            RuleFor(updateDealerRequest => updateDealerRequest.Business.Name)
                .NotNull();
            RuleFor(updateDealerRequest => updateDealerRequest.Business.Locations)
                .NotEmpty();
            RuleForEach(updateDealerRequest => updateDealerRequest.Business.Locations)
                .NotEmpty()
                .ChildRules(businessLocationRequestValidator =>
                    {
                        businessLocationRequestValidator
                            .RuleFor(businessLocation => businessLocation.OfficeName)
                            .NotEmpty();

                        businessLocationRequestValidator
                            .RuleFor(businessLocation => businessLocation.Address)
                            .NotEmpty();
                        businessLocationRequestValidator
                            .RuleFor(businessLocation => businessLocation.Address.City)
                            .NotEmpty();
                        businessLocationRequestValidator
                            .RuleFor(businessLocation => businessLocation.Address.State)
                            .NotEmpty();
                        businessLocationRequestValidator
                            .RuleFor(businessLocation => businessLocation.Address.ZipCode)
                            .NotEmpty();
                        businessLocationRequestValidator
                            .RuleFor(businessLocation => businessLocation.ServiceableCounties)
                            .NotEmpty();
                    }
                );
        });

        WhenAsync(async (updateDealerRequest, _, _) =>
            updateDealerRequest.OnboardingStep == DealerOnboardingStepEnum.PublicCompanyProfile
            && (jobCategoryResponses = await _catalogServiceClient.GetJobCategories()) != null, () =>
        {
            RuleFor(updateDealerRequest => updateDealerRequest.Business.About)
                .NotEmpty();
            RuleFor(updateDealerRequest => updateDealerRequest.Business.Brands)
                .NotEmpty();
            RuleFor(updateDealerRequest => updateDealerRequest.Business.Categories).NotEmpty().Custom(
                (value, context) =>
                {
                    if (value != null && jobCategoryResponses != null &&
                        !value.SelectMany(x => x.Codes)
                            .All(i => jobCategoryResponses.Select(x => x.Code).Contains(i)))
                    {
                        context.AddFailure(StaticValues.InvalidJobCategoryCode);
                    }
                });

            RuleFor(updateDealerRequest => updateDealerRequest.Business.StartYear).NotEmpty();
            RuleFor(updateDealerRequest => updateDealerRequest.Business.PoolCount).GreaterThanOrEqualTo(0)
                .LessThanOrEqualTo(StaticValues.MaxServicedPoolCountForDealer).NotEmpty();
            RuleFor(updateDealerRequest => updateDealerRequest.Business.WebsiteUrl).Custom(
                (value, context) =>
                {
                    if (!string.IsNullOrWhiteSpace(value) && !IsUrlValid(value))
                    {
                        context.AddFailure(StaticValues.InvalidWebSiteUrl);
                    }
                });
            RuleFor(updateDealerRequest => updateDealerRequest.Business.PhoneNumber).NotEmpty();
        });
    }

    private static bool IsUrlValid(string url)
    {
        const string pattern =
            "(https?://)?(www\\.)?[-a-zA-Z0-9@:%._\\+~#=]{2,256}\\.[a-z]{2,6}\\b([-a-zA-Z0-9@:%_\\+.~#()?&//=]*)";
        Regex reg = new(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        return reg.IsMatch(url);
    }
}
