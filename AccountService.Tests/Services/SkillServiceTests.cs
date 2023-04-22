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

public class SkillServiceTests
{
    private readonly IFixture _fixture;

    public SkillServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Customizations.Add(new RegularExpressionGenerator());
        _fixture.Customizations.Add(new RegularExpressionAttributeRelay());
        _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact(DisplayName =
        "Should be able to merge skills mapping when existing mappings are null and only incoming mappings are there with no other's data.")]
    public void CreateOrUpdateSkills_WithNoExistingData_IncomingMappingsNoOthersData_Success()
    {
        // arrange
        SkillRequest? incomingSkillRequest = _fixture.Build<SkillRequest>()
            .Without(x => x.Others)
            .With(x => x.Codes, new List<string> {"pump", "motor"})
            .Create();
        List<ExpertSkill> existingSkills = new();

        // act
        List<ExpertSkill> results =
            SkillService.CreateOrUpdateExpertSkills(existingSkills, incomingSkillRequest);

        // assert
        Assert.NotNull(results);
        Assert.Equal(2, results.Count);
        Assert.Equal(results.Select(x => x.Code), incomingSkillRequest.Codes);
        Assert.True(results.All(x => x.Others == null));
    }

    [Fact(DisplayName =
        "Should be able to merge skills mapping when existing mappings are null and only incoming mappings are there with other's data.")]
    public void CreateOrUpdateSkills_WithNoExistingData_IncomingMappingsWithOthersData_Success()
    {
        // arrange
        string skillOthersComment = "slippage, green water check";
        SkillRequest? incomingSkillRequest = _fixture.Build<SkillRequest>()
            .With(x => x.Others, skillOthersComment)
            .With(x => x.Codes, new List<string> {"pump", "motor", StaticValues.OthersCode})
            .Create();
        List<ExpertSkill> existingSkills = new();

        // act
        List<ExpertSkill> results =
            SkillService.CreateOrUpdateExpertSkills(existingSkills, incomingSkillRequest);

        // assert
        Assert.NotNull(results);
        Assert.Equal(3, results.Count);
        Assert.Equal(results.Select(x => x.Code), incomingSkillRequest.Codes);
        Assert.True(results.Count(x => x.Others != null) == 1);
        Assert.Equal(results.FirstOrDefault(x => x.Code == StaticValues.OthersCode)?.Others,
            skillOthersComment);
    }

    [Fact(DisplayName =
        "Should be able to merge skill mapping when existing mappings exists without others data and incoming mappings are there with no other's data.")]
    public void CreateOrUpdateSkills_WithExistingDataAndNoOthersData_IncomingMappingsWithNoOthersData_Success()
    {
        // arrange
        ExpertSkill? existingSkillComposition = _fixture.Build<ExpertSkill>()
            .With(x => x.Code, "water composition")
            .Without(x => x.Others)
            .Create();
        ExpertSkill? existingSkillChemistry = _fixture.Build<ExpertSkill>()
            .With(x => x.Code, "water chemistry")
            .Without(x => x.Others)
            .Create();
        List<ExpertSkill> existingSkills = new() {existingSkillComposition, existingSkillChemistry};

        SkillRequest? incomingSkillRequest = _fixture.Build<SkillRequest>()
            .Without(x => x.Others)
            .With(x => x.Codes, new List<string> {"pump", "motor"})
            .Create();

        // act
        List<ExpertSkill> results =
            SkillService.CreateOrUpdateExpertSkills(existingSkills, incomingSkillRequest);

        // assert
        Assert.NotNull(results);
        Assert.Equal(2, results.Count);
        Assert.True(results.All(x => x.Others == null));
        Assert.Equal(results.Select(x => x.Code), incomingSkillRequest.Codes);
    }

    [Fact(DisplayName =
        "Should be able to merge skill mapping when existing mappings exists with other's data and incoming mappings are there with no other's data.")]
    public void CreateOrUpdateSkills_WithExistingMappingsWithOthersData_IncomingMappingsWithNoOthersData_Success()
    {
        // arrange
        string skillOthersComment = "slippage, green water check";
        ExpertSkill? existingSkillComposition = _fixture.Build<ExpertSkill>()
            .With(x => x.Code, "water composition")
            .Without(x => x.Others)
            .Create();
        ExpertSkill? existingSkillChemistry = _fixture.Build<ExpertSkill>()
            .With(x => x.Code, "water chemistry")
            .Without(x => x.Others)
            .Create();
        ExpertSkill? existingSkillOthers = _fixture.Build<ExpertSkill>()
            .With(x => x.Code, StaticValues.OthersCode)
            .With(x => x.Others, skillOthersComment)
            .Create();
        List<ExpertSkill> existingSkills = new()
            {existingSkillComposition, existingSkillChemistry, existingSkillOthers};

        SkillRequest? incomingSkillRequest = _fixture.Build<SkillRequest>()
            .Without(x => x.Others)
            .With(x => x.Codes, new List<string> {"motor", "pump"})
            .Create();

        // act
        List<ExpertSkill> results =
            SkillService.CreateOrUpdateExpertSkills(existingSkills, incomingSkillRequest);

        // assert
        Assert.NotNull(results);
        Assert.Equal(2, results.Count);
        Assert.True(results.All(x => x.Others == null));
        Assert.Equal(results.Select(x => x.Code), incomingSkillRequest.Codes);
        Assert.Null(results.FirstOrDefault(x => x.Code == StaticValues.OthersCode));
    }

    [Fact(DisplayName =
        "Should be able to merge skills mapping when existing mappings exists without others data and incoming mappings are there with other's data.")]
    public void CreateOrUpdateSkills_WithExistingDataAndNoOthersData_IncomingMappingsWithOthersData_Success()
    {
        // arrange
        string skillOthersComment = "slippage, green water check";
        ExpertSkill? existingSkillComposition = _fixture.Build<ExpertSkill>()
            .With(x => x.Code, "water composition")
            .Without(x => x.Others)
            .Create();
        ExpertSkill? existingSkillChemistry = _fixture.Build<ExpertSkill>()
            .With(x => x.Code, "water chemistry")
            .Without(x => x.Others)
            .Create();
        List<ExpertSkill> existingSkills = new() {existingSkillComposition, existingSkillChemistry};

        SkillRequest? incomingSkillRequest = _fixture.Build<SkillRequest>()
            .With(x => x.Others, skillOthersComment)
            .With(x => x.Codes, new List<string> {"motor", "pump", StaticValues.OthersCode})
            .Create();

        // act
        List<ExpertSkill> results =
            SkillService.CreateOrUpdateExpertSkills(existingSkills, incomingSkillRequest);

        // assert
        Assert.NotNull(results);
        Assert.Equal(3, results.Count);
        Assert.True(results.Count(x => x.Others != null) == 1);
        Assert.Equal(results.Select(x => x.Code), incomingSkillRequest.Codes);
        Assert.Equal(results.FirstOrDefault(x => x.Code == StaticValues.OthersCode)?.Others,
            skillOthersComment);
    }

    [Fact(DisplayName =
        "Should be able to merge skill mapping when existing mappings exists with other's data and incoming mappings are there with other's data.")]
    public void CreateOrUpdateSkills_WithExistingMappingsWithOthersData_IncomingMappingsWithOthersData_Success()
    {
        // arrange
        string oldSkillOthersComment = "slippage, green water check";
        ExpertSkill? existingSkillComposition = _fixture.Build<ExpertSkill>()
            .With(x => x.Code, "water composition")
            .Without(x => x.Others)
            .Create();
        ExpertSkill? existingSkillChemistry = _fixture.Build<ExpertSkill>()
            .With(x => x.Code, "water chemistry")
            .Without(x => x.Others)
            .Create();
        ExpertSkill? existingSkillOthers = _fixture.Build<ExpertSkill>()
            .With(x => x.Code, StaticValues.OthersCode)
            .With(x => x.Others, oldSkillOthersComment)
            .Create();
        List<ExpertSkill> existingSkills = new()
            {existingSkillComposition, existingSkillChemistry, existingSkillOthers};

        string newSkillOthersComment = "water chlorine check";
        SkillRequest? incomingSkillRequest = _fixture.Build<SkillRequest>()
            .With(x => x.Others, newSkillOthersComment)
            .With(x => x.Codes, new List<string> {"motor", "pump", StaticValues.OthersCode})
            .Create();

        // act
        List<ExpertSkill> results =
            SkillService.CreateOrUpdateExpertSkills(existingSkills, incomingSkillRequest);

        // assert
        Assert.NotNull(results);
        Assert.Equal(3, results.Count);
        Assert.Equal(results.Select(x => x.Code), incomingSkillRequest.Codes);
        Assert.True(results.Count(x => x.Others != null) == 1);
        Assert.Equal(results.FirstOrDefault(x => x.Code == StaticValues.OthersCode)?.Others,
            newSkillOthersComment);
    }
}
