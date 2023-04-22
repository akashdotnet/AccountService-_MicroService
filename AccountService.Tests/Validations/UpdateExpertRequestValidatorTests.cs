using System.Collections.Generic;
using System.Threading.Tasks;
using AccountService.API.Clients.Interfaces;
using AccountService.API.Dto.Request;
using AccountService.API.Dto.Response;
using AccountService.API.Validations;
using AutoFixture;
using AutoFixture.DataAnnotations;
using FluentValidation.TestHelper;
using Moq;
using Xunit;

namespace AccountService.API.Tests.Validations;

public class UpdateExpertRequestValidatorTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ICatalogServiceClient> _mockCatalogServiceClient;
    private readonly UpdateExpertRequestValidator _validator;

    public UpdateExpertRequestValidatorTests()
    {
        _fixture = new Fixture();
        //add in the ordered customizations (put derived ones before base ones)
        _fixture.Customizations.Add(new RegularExpressionGenerator());
        _fixture.Customizations.Add(new RegularExpressionAttributeRelay());
        _mockCatalogServiceClient = new Mock<ICatalogServiceClient>();
        _validator = new UpdateExpertRequestValidator(_mockCatalogServiceClient.Object);
        SetUpMockServices();
    }

    [Fact(DisplayName = "Should validate expert languages successfully.")]
    public async Task UpdateExpert_WithLanguagesWithoutSkills_ShouldValidateSuccessfully()
    {
        // arrange
        UpdateExpertRequest updateExpertRequestObj = _fixture.Build<UpdateExpertRequest>()
            .With(x => x.Languages, _fixture.Build<LanguageRequest>()
                .With(x => x.Codes, new List<string> {"english"})
                .Create())
            .Without(x => x.Skills)
            .Create();

        // act
        TestValidationResult<UpdateExpertRequest>? result =
            await _validator.TestValidateAsync(updateExpertRequestObj);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact(DisplayName = "Should validate expert skills and languages successfully.")]
    public async Task UpdateExpert_WithLanguagesWithSkills_ShouldValidateSuccessfully()
    {
        // arrange                                                                                 
        UpdateExpertRequest updateExpertRequestObj = _fixture.Build<UpdateExpertRequest>()
            .With(x => x.Skills, _fixture.Build<SkillRequest>()
                .With(x => x.Codes, new List<string> {"motor"})
                .Create())
            .With(x => x.Languages, _fixture.Build<LanguageRequest>()
                .With(x => x.Codes, new List<string> {"english"})
                .Create())
            .Create();

        // act                                                                                     
        TestValidationResult<UpdateExpertRequest>? result =
            await _validator.TestValidateAsync(updateExpertRequestObj);

        // assert                                                                                  
        Assert.True(result.IsValid);
    }

    [Fact(DisplayName = "Should validate expert skills successfully.")]
    public async Task UpdateExpert_WithoutLanguagesWithSkills_ShouldValidateSuccessfully()
    {
        // arrange                                                                               
        UpdateExpertRequest updateExpertRequestObj = _fixture.Build<UpdateExpertRequest>()
            .With(x => x.Skills, _fixture.Build<SkillRequest>()
                .With(x => x.Codes, new List<string> {"motor"})
                .Create())
            .Without(x => x.Languages)
            .Create();

        // act                                                                                   
        TestValidationResult<UpdateExpertRequest>? result =
            await _validator.TestValidateAsync(updateExpertRequestObj);

        // assert                                                                                
        Assert.True(result.IsValid);
    }

    [Fact(DisplayName = "Should not throw any error when language and skill request is null in the request.")]
    public async Task UpdateExpert_WithoutLanguageAndSkills_ShouldValidateSuccessfully()
    {
        // arrange                                                                             
        UpdateExpertRequest updateExpertRequestObj = _fixture.Build<UpdateExpertRequest>()
            .Without(x => x.Languages)
            .Without(x => x.Skills)
            .Create();

        // act                                                                                 
        TestValidationResult<UpdateExpertRequest>? result =
            await _validator.TestValidateAsync(updateExpertRequestObj);

        // assert                                                                              
        Assert.True(result.IsValid);
    }


    [Fact(DisplayName = "Should validate expert languages successfully when skills are empty.")]
    public async Task UpdateExpert_WithLanguagesWithEmptySkills_ShouldValidateSuccessfully()
    {
        // arrange                                                                                          
        UpdateExpertRequest updateExpertRequestObj = _fixture.Build<UpdateExpertRequest>()
            .With(x => x.Languages, _fixture.Build<LanguageRequest>()
                .With(x => x.Codes, new List<string> {"english"})
                .Create())
            .With(x => x.Skills, _fixture.Build<SkillRequest>()
                .With(x => x.Codes, new List<string>())
                .Create())
            .Create();

        // act                                                                                              
        TestValidationResult<UpdateExpertRequest>? result =
            await _validator.TestValidateAsync(updateExpertRequestObj);

        // assert                                                                                           
        Assert.True(result.IsValid);
    }

    [Fact(DisplayName = "Should validate expert skills successfully when languages are empty.")]
    public async Task UpdateExpert_WithEmptyLanguagesWithSkills_ShouldValidateSuccessfully()
    {
        // arrange                                                                                          
        UpdateExpertRequest updateExpertRequestObj = _fixture.Build<UpdateExpertRequest>()
            .With(x => x.Skills, _fixture.Build<SkillRequest>()
                .With(x => x.Codes, new List<string> {"motor"})
                .Create())
            .With(x => x.Languages, _fixture.Build<LanguageRequest>()
                .With(x => x.Codes, new List<string>())
                .Create())
            .Create();

        // act                                                                                              
        TestValidationResult<UpdateExpertRequest>? result =
            await _validator.TestValidateAsync(updateExpertRequestObj);

        // assert                                                                                           
        Assert.True(result.IsValid);
    }

    [Fact(DisplayName = "Should not throw any error when language and skills both are empty in the request.")]
    public async Task UpdateExpert_WithEmptyLanguagesAndEmptySkills_ShouldValidateSuccessfully()
    {
        // arrange                                                                                          
        UpdateExpertRequest updateExpertRequestObj = _fixture.Build<UpdateExpertRequest>()
            .With(x => x.Skills, _fixture.Build<SkillRequest>()
                .With(x => x.Codes, new List<string>())
                .Create())
            .With(x => x.Languages, _fixture.Build<LanguageRequest>()
                .With(x => x.Codes, new List<string>())
                .Create())
            .Create();

        // act                                                                                              
        TestValidationResult<UpdateExpertRequest>? result =
            await _validator.TestValidateAsync(updateExpertRequestObj);

        // assert                                                                                           
        Assert.True(result.IsValid);
    }

    [Fact(DisplayName = "Should throw validation error for language when its invalid.")]
    public async Task UpdateExpert_WithInvalidLanguagesAndValidSkills_ShouldThrowError()
    {
        // arrange                                                                                               
        UpdateExpertRequest updateExpertRequestObj = _fixture.Build<UpdateExpertRequest>()
            .With(x => x.Skills, _fixture.Build<SkillRequest>()
                .With(x => x.Codes, new List<string> {"motor"})
                .Create())
            .With(x => x.Languages, _fixture.Build<LanguageRequest>()
                .With(x => x.Codes, new List<string> {"french"})
                .Create())
            .Create();

        // act                                                                                                   
        TestValidationResult<UpdateExpertRequest>? result =
            await _validator.TestValidateAsync(updateExpertRequestObj);

        // assert                                                                                                
        Assert.False(result.IsValid);
        result.ShouldHaveValidationErrorFor(x => x.Languages);
        result.ShouldNotHaveValidationErrorFor(x => x.Skills);
    }

    [Fact(DisplayName = "Should throw validation error for skills when its invalid.")]
    public async Task UpdateExpert_WithValidLanguagesAndInvalidSkills_ShouldThrowError()
    {
        // arrange                                                                                              
        UpdateExpertRequest updateExpertRequestObj = _fixture.Build<UpdateExpertRequest>()
            .With(x => x.Skills, _fixture.Build<SkillRequest>()
                .With(x => x.Codes, new List<string> {"chemistry"})
                .Create())
            .With(x => x.Languages, _fixture.Build<LanguageRequest>()
                .With(x => x.Codes, new List<string> {"english"})
                .Create())
            .Create();

        // act                                                                                                  
        TestValidationResult<UpdateExpertRequest>? result =
            await _validator.TestValidateAsync(updateExpertRequestObj);

        // assert                                                                                               
        Assert.False(result.IsValid);
        result.ShouldHaveValidationErrorFor(x => x.Skills);
        result.ShouldNotHaveValidationErrorFor(x => x.Languages);
    }

    [Fact(DisplayName = "Should throw validation error for language and skill when its invalid.")]
    public async Task UpdateExpert_WithInvalidLanguagesAndInvalidSkills_ShouldThrowError()
    {
        // arrange                                                                                              
        UpdateExpertRequest updateExpertRequestObj = _fixture.Build<UpdateExpertRequest>()
            .With(x => x.Skills, _fixture.Build<SkillRequest>()
                .With(x => x.Codes, new List<string> {"water chemistry"})
                .Create())
            .With(x => x.Languages, _fixture.Build<LanguageRequest>()
                .With(x => x.Codes, new List<string> {"french"})
                .Create())
            .Create();

        // act                                                                                                  
        TestValidationResult<UpdateExpertRequest>? result =
            await _validator.TestValidateAsync(updateExpertRequestObj);

        // assert                                                                                               
        Assert.False(result.IsValid);
        result.ShouldHaveValidationErrorFor(x => x.Languages);
        result.ShouldHaveValidationErrorFor(x => x.Skills);
    }

    private void SetUpMockServices()
    {
        List<LanguageResponse> languageResponses = new();
        LanguageResponse languageResponse = _fixture.Build<LanguageResponse>()
            .With(x => x.Code, "english").Create();
        languageResponses.Add(languageResponse);

        List<SkillResponse> skillResponses = new();
        SkillResponse skillResponse = _fixture.Build<SkillResponse>()
            .With(x => x.Code, "motor").Create();
        skillResponses.Add(skillResponse);

        _mockCatalogServiceClient.Setup(x => x.GetLanguages()).ReturnsAsync(languageResponses);
        _mockCatalogServiceClient.Setup(x => x.GetSkills()).ReturnsAsync(skillResponses);
    }
}
