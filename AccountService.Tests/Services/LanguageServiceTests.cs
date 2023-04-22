using System.Collections.Generic;
using System.Linq;
using AccountService.API.Constants;
using AccountService.API.Dto.Request;
using AccountService.API.Models;
using AccountService.API.Services;
using AutoFixture;
using AutoFixture.DataAnnotations;
using Xunit;

namespace AccountService.API.Tests.Services;

public class LanguageServiceTests
{
    private readonly IFixture _fixture;

    public LanguageServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Customizations.Add(new RegularExpressionGenerator());
        _fixture.Customizations.Add(new RegularExpressionAttributeRelay());
        _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact(DisplayName =
        "Should be able to merge language mapping when existing mappings are null and only incoming mappings are there with no other's data.")]
    public void CreateOrUpdateLanguages_WithNoExistingData_IncomingMappingsNoOthersData_Success()
    {
        // arrange
        LanguageRequest? incomingLanguageRequest = _fixture.Build<LanguageRequest>()
            .Without(x => x.Others)
            .With(x => x.Codes, new List<string> {"english", "french"})
            .Create();
        List<ExpertLanguage> existingLanguages = new();

        // act
        List<ExpertLanguage> results =
            LanguageService.CreateOrUpdateExpertLanguages(existingLanguages, incomingLanguageRequest);

        // assert
        Assert.NotNull(results);
        Assert.Equal(2, results.Count);
        Assert.Equal(results.Select(x => x.Code), incomingLanguageRequest.Codes);
        Assert.True(results.All(x => x.Others == null));
    }

    [Fact(DisplayName =
        "Should be able to merge language mapping when existing mappings are null and only incoming mappings are there with other's data.")]
    public void CreateOrUpdateLanguages_WithNoExistingData_IncomingMappingsWithOthersData_Success()
    {
        // arrange
        string languageOthersComment = "hindi, spanish, chinese";
        LanguageRequest? incomingLanguageRequest = _fixture.Build<LanguageRequest>()
            .With(x => x.Others, languageOthersComment)
            .With(x => x.Codes, new List<string> {"english", "french", StaticValues.OthersCode})
            .Create();
        List<ExpertLanguage> existingLanguages = new();

        // act
        List<ExpertLanguage> results =
            LanguageService.CreateOrUpdateExpertLanguages(existingLanguages, incomingLanguageRequest);

        // assert
        Assert.NotNull(results);
        Assert.Equal(3, results.Count);
        Assert.Equal(results.Select(x => x.Code), incomingLanguageRequest.Codes);
        Assert.True(results.Count(x => x.Others != null) == 1);
        Assert.Equal(results.FirstOrDefault(x => x.Code == StaticValues.OthersCode)?.Others,
            languageOthersComment);
    }

    [Fact(DisplayName =
        "Should be able to merge language mapping when existing mappings exists without others data and incoming mappings are there with no other's data.")]
    public void CreateOrUpdateLanguages_WithExistingDataAndNoOthersData_IncomingMappingsWithNoOthersData_Success()
    {
        // arrange
        ExpertLanguage? existingLanguageEnglish = _fixture.Build<ExpertLanguage>()
            .With(x => x.Code, "english")
            .Without(x => x.Others)
            .Create();
        ExpertLanguage? existingLanguageSpanish = _fixture.Build<ExpertLanguage>()
            .With(x => x.Code, "spanish")
            .Without(x => x.Others)
            .Create();
        List<ExpertLanguage> existingLanguages = new() {existingLanguageEnglish, existingLanguageSpanish};

        LanguageRequest? incomingLanguageRequest = _fixture.Build<LanguageRequest>()
            .Without(x => x.Others)
            .With(x => x.Codes, new List<string> {"english", "french"})
            .Create();

        // act
        List<ExpertLanguage> results =
            LanguageService.CreateOrUpdateExpertLanguages(existingLanguages, incomingLanguageRequest);

        // assert
        Assert.NotNull(results);
        Assert.Equal(2, results.Count);
        Assert.True(results.All(x => x.Others == null));
        Assert.Equal(results.Select(x => x.Code), incomingLanguageRequest.Codes);
    }

    [Fact(DisplayName =
        "Should be able to merge language mapping when existing mappings exists with other's data and incoming mappings are there with no other's data.")]
    public void CreateOrUpdateLanguages_WithExistingMappingsWithOthersData_IncomingMappingsWithNoOthersData_Success()
    {
        // arrange
        string languageOthersComment = "hindi, spanish, chinese";
        ExpertLanguage? existingLanguageEnglish = _fixture.Build<ExpertLanguage>()
            .With(x => x.Code, "english")
            .Without(x => x.Others)
            .Create();
        ExpertLanguage? existingLanguageSpanish = _fixture.Build<ExpertLanguage>()
            .With(x => x.Code, "spanish")
            .Without(x => x.Others)
            .Create();
        ExpertLanguage? existingLanguageOthers = _fixture.Build<ExpertLanguage>()
            .With(x => x.Code, StaticValues.OthersCode)
            .With(x => x.Others, languageOthersComment)
            .Create();
        List<ExpertLanguage> existingLanguages = new()
            {existingLanguageEnglish, existingLanguageSpanish, existingLanguageOthers};

        LanguageRequest? incomingLanguageRequest = _fixture.Build<LanguageRequest>()
            .Without(x => x.Others)
            .With(x => x.Codes, new List<string> {"english", "french"})
            .Create();

        // act
        List<ExpertLanguage> results =
            LanguageService.CreateOrUpdateExpertLanguages(existingLanguages, incomingLanguageRequest);

        // assert
        Assert.NotNull(results);
        Assert.Equal(2, results.Count);
        Assert.True(results.All(x => x.Others == null));
        Assert.Equal(results.Select(x => x.Code), incomingLanguageRequest.Codes);
        Assert.Null(results.FirstOrDefault(x => x.Code == StaticValues.OthersCode));
    }

    [Fact(DisplayName =
        "Should be able to merge language mapping when existing mappings exists without others data and incoming mappings are there with other's data.")]
    public void CreateOrUpdateLanguages_WithExistingDataAndNoOthersData_IncomingMappingsWithOthersData_Success()
    {
        // arrange
        string languageOthersComment = "hindi, spanish, chinese";
        ExpertLanguage? existingLanguageEnglish = _fixture.Build<ExpertLanguage>()
            .With(x => x.Code, "english")
            .Without(x => x.Others)
            .Create();
        ExpertLanguage? existingLanguageSpanish = _fixture.Build<ExpertLanguage>()
            .With(x => x.Code, "spanish")
            .Without(x => x.Others)
            .Create();
        List<ExpertLanguage> existingLanguages = new() {existingLanguageEnglish, existingLanguageSpanish};

        LanguageRequest? incomingLanguageRequest = _fixture.Build<LanguageRequest>()
            .With(x => x.Others, languageOthersComment)
            .With(x => x.Codes, new List<string> {"english", "french", StaticValues.OthersCode})
            .Create();

        // act
        List<ExpertLanguage> results =
            LanguageService.CreateOrUpdateExpertLanguages(existingLanguages, incomingLanguageRequest);

        // assert
        Assert.NotNull(results);
        Assert.Equal(3, results.Count);
        Assert.True(results.Count(x => x.Others != null) == 1);
        Assert.Equal(results.Select(x => x.Code), incomingLanguageRequest.Codes);
        Assert.Equal(results.FirstOrDefault(x => x.Code == StaticValues.OthersCode)?.Others,
            languageOthersComment);
    }

    [Fact(DisplayName =
        "Should be able to merge language mapping when existing mappings exists with other's data and incoming mappings are there with other's data.")]
    public void CreateOrUpdateLanguages_WithExistingMappingsWithOthersData_IncomingMappingsWithOthersData_Success()
    {
        // arrange
        string oldLanguageOthersComment = "hindi, spanish, chinese";
        ExpertLanguage? existingLanguageEnglish = _fixture.Build<ExpertLanguage>()
            .With(x => x.Code, "english")
            .Without(x => x.Others)
            .Create();
        ExpertLanguage? existingLanguageSpanish = _fixture.Build<ExpertLanguage>()
            .With(x => x.Code, "spanish")
            .Without(x => x.Others)
            .Create();
        ExpertLanguage? existingLanguageOthers = _fixture.Build<ExpertLanguage>()
            .With(x => x.Code, StaticValues.OthersCode)
            .With(x => x.Others, oldLanguageOthersComment)
            .Create();
        List<ExpertLanguage> existingLanguages = new()
            {existingLanguageEnglish, existingLanguageSpanish, existingLanguageOthers};

        string newLanguageOthersComment = "hindi, spanish, russian";
        LanguageRequest? incomingLanguageRequest = _fixture.Build<LanguageRequest>()
            .With(x => x.Others, newLanguageOthersComment)
            .With(x => x.Codes, new List<string> {"english", "french", StaticValues.OthersCode})
            .Create();

        // act
        List<ExpertLanguage> results =
            LanguageService.CreateOrUpdateExpertLanguages(existingLanguages, incomingLanguageRequest);

        // assert
        Assert.NotNull(results);
        Assert.Equal(3, results.Count);
        Assert.Equal(results.Select(x => x.Code), incomingLanguageRequest.Codes);
        Assert.True(results.Count(x => x.Others != null) == 1);
        Assert.Equal(results.FirstOrDefault(x => x.Code == StaticValues.OthersCode)?.Others,
            newLanguageOthersComment);
    }
}
