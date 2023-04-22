using System;
using System.Collections.Generic;
using System.Linq;
using AccountService.API.Clients.Interfaces;
using AccountService.API.Constants;
using AccountService.API.Dto.Request;
using AccountService.API.Dto.Response;
using FluentValidation;

namespace AccountService.API.Validations;

public class UpdateExpertRequestValidator : AbstractValidator<UpdateExpertRequest>
{
    private readonly ICatalogServiceClient _catalogServiceClient;

    public UpdateExpertRequestValidator(ICatalogServiceClient catalogServiceClient)
    {
        _catalogServiceClient = catalogServiceClient ?? throw new ArgumentNullException(nameof(catalogServiceClient));
        List<SkillResponse> skillResponses = new();
        List<LanguageResponse> languageResponses = new();

        WhenAsync(async (updateExpertRequest, context, cancellation) =>
            //updateExpertRequest.Languages != null && updateExpertRequest.Languages.LanguageCodes.Any()
            (skillResponses = await _catalogServiceClient.GetSkills()) != null
            && (languageResponses = await _catalogServiceClient.GetLanguages()) != null, () =>
        {
            RuleFor(updateExpertRequest => updateExpertRequest.Languages).Custom((value, context) =>
            {
                if (value != null && value.Codes.Any() && languageResponses != null &&
                    !value.Codes
                        .All(i => languageResponses.Select(x => x.Code).Contains(i)))
                {
                    context.AddFailure(StaticValues.InvalidLanguageCode);
                }
            });

            RuleFor(updateExpertRequest => updateExpertRequest.Skills).Custom((value, context) =>
            {
                if (value != null && value.Codes.Any() && skillResponses != null &&
                    !value.Codes
                        .All(i => skillResponses.Select(x => x.Code).Contains(i)))
                {
                    context.AddFailure(StaticValues.InvalidSkillCode);
                }
            });
        });
    }
}
