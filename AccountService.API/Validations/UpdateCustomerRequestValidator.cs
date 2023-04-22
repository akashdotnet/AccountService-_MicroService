using System;
using System.Linq;
using AccountService.API.Clients.Interfaces;
using AccountService.API.Constants;
using AccountService.API.Dto.CatalogServiceClient;
using AccountService.API.Dto.Request;
using AccountService.API.Enums;
using FluentValidation;

namespace AccountService.API.Validations;

public class UpdateCustomerRequestValidator : AbstractValidator<UpdateCustomerRequest>
{
    private readonly ICatalogServiceClient _catalogServiceClient;

    public UpdateCustomerRequestValidator(ICatalogServiceClient catalogServiceClient)
    {
        _catalogServiceClient = catalogServiceClient ?? throw new ArgumentNullException(nameof(catalogServiceClient));
        PoolDetailLookups poolDetailLookupsData = new();

        When(customer => customer.OnboardingStep == CustomerStepEnum.GettingStarted,
            () =>
            {
                RuleFor(customer => customer.Address).NotEmpty().ChildRules(addressValidator =>
                {
                    addressValidator.RuleFor(address => address.State).NotEmpty();
                    addressValidator.RuleFor(address => address.ZipCode).NotEmpty();
                    addressValidator.RuleFor(address => address.City).NotEmpty();
                    addressValidator.RuleFor(address => address.AddressValue).NotEmpty();
                });
                RuleFor(customer => customer.PhoneNumber).NotEmpty();
            }
        );

        WhenAsync(async (customer, _, _) =>
            customer.OnboardingStep is CustomerStepEnum.PoolDetails or CustomerStepEnum.EditProfile
            && (poolDetailLookupsData = await _catalogServiceClient.GetPoolDetailLookups()) != null, () =>
        {
            RuleFor(customer => customer.PoolMaterialCode).Custom((value, context) =>
            {
                if (!string.IsNullOrWhiteSpace(value) && poolDetailLookupsData.PoolMaterials != null &&
                    !poolDetailLookupsData.PoolMaterials.Select(x => x.Code).Contains(value))
                {
                    context.AddFailure(StaticValues.InvalidPoolMaterialCode);
                }
            });

            RuleFor(customer => customer.PoolSizeCode).Custom((value, context) =>
            {
                if (!string.IsNullOrWhiteSpace(value) && poolDetailLookupsData.PoolSizes != null &&
                    !poolDetailLookupsData.PoolSizes.Select(x => x.Code).Contains(value))
                {
                    context.AddFailure(StaticValues.InvalidPoolSizeCode);
                }
            });

            RuleFor(customer => customer.PoolTypeCode).Custom((value, context) =>
            {
                if (!string.IsNullOrWhiteSpace(value) && poolDetailLookupsData.PoolTypes != null &&
                    !poolDetailLookupsData.PoolTypes.Select(x => x.Code).Contains(value))
                {
                    context.AddFailure(StaticValues.InvalidPoolTypeCode);
                }
            });

            RuleFor(customer => customer.SanitationMethodCode).Custom((value, context) =>
            {
                if (!string.IsNullOrWhiteSpace(value) && poolDetailLookupsData.SanitationMethods != null &&
                    !poolDetailLookupsData.SanitationMethods.Select(x => x.Code).Contains(value))
                {
                    context.AddFailure(StaticValues.InvalidSanitationMethodCode);
                }
            });

            RuleFor(customer => customer.PoolSeasonCode).Custom((value, context) =>
            {
                if (!string.IsNullOrWhiteSpace(value) && poolDetailLookupsData.PoolSeasons != null &&
                    !poolDetailLookupsData.PoolSeasons.Select(x => x.Code).Contains(value))
                {
                    context.AddFailure(StaticValues.InvalidPoolSeasonCode);
                }
            });

            RuleFor(customer => customer.HotTubTypeCode).Custom((value, context) =>
            {
                if (!string.IsNullOrWhiteSpace(value) && poolDetailLookupsData.HotTubTypes != null &&
                    !poolDetailLookupsData.HotTubTypes.Select(x => x.Code).Contains(value))
                {
                    context.AddFailure(StaticValues.InvalidHotTubTypeCode);
                }
            });
        });
    }
}
