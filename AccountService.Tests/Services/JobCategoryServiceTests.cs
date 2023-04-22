using System.Collections.Generic;
using System.Linq;
using AccountService.API.Dto.Request;
using AccountService.API.Enums;
using AccountService.API.Models;
using AccountService.API.Services;
using AutoFixture;
using AutoFixture.DataAnnotations;
using Xunit;

namespace AccountService.API.Tests.Services;

public class JobCategoryServiceTests
{
    private readonly IFixture _fixture;

    public JobCategoryServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Customizations.Add(new RegularExpressionGenerator());
        _fixture.Customizations.Add(new RegularExpressionAttributeRelay());
        _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }


    [Fact(DisplayName =
        "Should be able to merge job categories mapping when existing mappings are null and only incoming mappings are there with no other's data.")]
    public void CreateOrUpdateJobCategories_WithNoExistingData_IncomingMappingsNoOthersData_Success()
    {
        List<BusinessJobCategory> existingCategories = new();
        JobCategoriesRequest? incomingCategoryOne = _fixture.Build<JobCategoriesRequest>()
            .Without(x => x.Others)
            .With(x => x.Type, JobCategoryTypeEnum.Repair)
            .With(x => x.Codes, new List<string> {"A", "B", "C"})
            .Create();
        JobCategoriesRequest? incomingCategoryTwo = _fixture.Build<JobCategoriesRequest>()
            .Without(x => x.Others)
            .With(x => x.Type, JobCategoryTypeEnum.Service)
            .With(x => x.Codes, new List<string> {"X", "Y", "Z"})
            .Create();
        List<JobCategoriesRequest> incomingCategories = new() {incomingCategoryOne, incomingCategoryTwo};

        List<BusinessJobCategory> results =
            JobCategoryService.CreateOrUpdateJobCategories(existingCategories, incomingCategories);

        Assert.NotNull(results);
        Assert.Equal(6, results.Count);
        Assert.Equal(results.Select(x => x.Code), incomingCategories.SelectMany(x => x.Codes));
    }

    [Fact(DisplayName =
        "Should be able to merge job categories mapping when existing mappings are null and only incoming mappings are there with repair type other's data.")]
    public void CreateOrUpdateJobCategories_WithNoExistingData_IncomingMappingsWithRepairOthersData_Success()
    {
        string repairOtherCategory = "repair_other";
        string repairOtherCategoryComments = "repair other comments 1, repair other comments 2";
        List<BusinessJobCategory> existingCategories = new();
        JobCategoriesRequest? incomingCategoryOne = _fixture.Build<JobCategoriesRequest>()
            .With(x => x.Others, repairOtherCategoryComments)
            .With(x => x.Type, JobCategoryTypeEnum.Repair)
            .With(x => x.Codes, new List<string> {"A", "B", "C", repairOtherCategory})
            .Create();
        JobCategoriesRequest? incomingCategoryTwo = _fixture.Build<JobCategoriesRequest>()
            .Without(x => x.Others)
            .With(x => x.Type, JobCategoryTypeEnum.Service)
            .With(x => x.Codes, new List<string> {"X", "Y", "Z"})
            .Create();
        List<JobCategoriesRequest> incomingCategories = new() {incomingCategoryOne, incomingCategoryTwo};

        List<BusinessJobCategory> results =
            JobCategoryService.CreateOrUpdateJobCategories(existingCategories, incomingCategories);

        Assert.NotNull(results);
        Assert.Equal(7, results.Count);
        Assert.Equal(results.Select(x => x.Code).OrderBy(x => x),
            incomingCategories.SelectMany(x => x.Codes).OrderBy(x => x));
        Assert.Equal(results.FirstOrDefault(x => x.Code == repairOtherCategory)?.Others,
            repairOtherCategoryComments);
    }

    [Fact(DisplayName =
        "Should be able to merge job categories mapping when existing mappings are null and only incoming mappings are there with service type other's data.")]
    public void CreateOrUpdateJobCategories_WithNoExistingData_IncomingMappingsWithServiceOthersData_Success()
    {
        string serviceOtherCategory = "service_other";
        string serviceOtherCategoryComments = "service other comments 1, service other comments 2";
        List<BusinessJobCategory> existingCategories = new();
        JobCategoriesRequest? incomingCategoryOne = _fixture.Build<JobCategoriesRequest>()
            .Without(x => x.Others)
            .With(x => x.Type, JobCategoryTypeEnum.Repair)
            .With(x => x.Codes, new List<string> {"A", "B", "C"})
            .Create();
        JobCategoriesRequest? incomingCategoryTwo = _fixture.Build<JobCategoriesRequest>()
            .With(x => x.Others, serviceOtherCategoryComments)
            .With(x => x.Type, JobCategoryTypeEnum.Service)
            .With(x => x.Codes, new List<string> {"X", "Y", "Z", serviceOtherCategory})
            .Create();
        List<JobCategoriesRequest> incomingCategories = new() {incomingCategoryOne, incomingCategoryTwo};

        List<BusinessJobCategory> results =
            JobCategoryService.CreateOrUpdateJobCategories(existingCategories, incomingCategories);

        Assert.NotNull(results);
        Assert.Equal(7, results.Count);
        Assert.Equal(results.Select(x => x.Code).OrderBy(x => x),
            incomingCategories.SelectMany(x => x.Codes).OrderBy(x => x));
        Assert.Equal(results.FirstOrDefault(x => x.Code == serviceOtherCategory)?.Others,
            serviceOtherCategoryComments);
    }

    [Fact(DisplayName =
        "Should be able to merge job categories mapping when existing mappings are null and only incoming mappings are there with both service/repair type other's data.")]
    public void CreateOrUpdateJobCategories_WithNoExistingData_IncomingMappingsWithServiceAndRepairOthersData_Success()
    {
        string serviceOtherCategory = "service_other";
        string serviceOtherCategoryComments = "service other comments 1, service other comments 2";
        string repairOtherCategory = "repair_other";
        string repairOtherCategoryComments = "repair other comments 1, repair other comments 2";
        List<BusinessJobCategory> existingCategories = new();
        JobCategoriesRequest? incomingCategoryOne = _fixture.Build<JobCategoriesRequest>()
            .With(x => x.Others, repairOtherCategoryComments)
            .With(x => x.Type, JobCategoryTypeEnum.Repair)
            .With(x => x.Codes, new List<string> {"A", "B", "C", repairOtherCategory})
            .Create();
        JobCategoriesRequest? incomingCategoryTwo = _fixture.Build<JobCategoriesRequest>()
            .With(x => x.Others, serviceOtherCategoryComments)
            .With(x => x.Type, JobCategoryTypeEnum.Service)
            .With(x => x.Codes, new List<string> {"X", "Y", "Z", serviceOtherCategory})
            .Create();
        List<JobCategoriesRequest> incomingCategories = new() {incomingCategoryOne, incomingCategoryTwo};

        List<BusinessJobCategory> results =
            JobCategoryService.CreateOrUpdateJobCategories(existingCategories, incomingCategories);

        Assert.NotNull(results);
        Assert.Equal(8, results.Count);
        Assert.Equal(results.Select(x => x.Code).OrderBy(x => x),
            incomingCategories.SelectMany(x => x.Codes).OrderBy(x => x));
        Assert.Equal(results.FirstOrDefault(x => x.Code == repairOtherCategory)?.Others,
            repairOtherCategoryComments);
        Assert.Equal(results.FirstOrDefault(x => x.Code == serviceOtherCategory)?.Others,
            serviceOtherCategoryComments);
    }

    [Fact(DisplayName =
        "Should be able to merge job categories mapping when existing mappings exist and incoming mappings are there with no other's data. Common mappings should update and new from incoming should add.")]
    public void
        CreateOrUpdateJobCategories_WithExistingData_IncomingMappingsWithNoOthersData_ShouldAddNewMappings_Success()
    {
        BusinessJobCategory? existingCategoryOne = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, "A").Create();
        BusinessJobCategory? existingCategoryTwo = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, "X").Create();
        List<BusinessJobCategory> existingCategories = new() {existingCategoryOne, existingCategoryTwo};
        JobCategoriesRequest? incomingCategoryOne = _fixture.Build<JobCategoriesRequest>()
            .Without(x => x.Others)
            .With(x => x.Type, JobCategoryTypeEnum.Repair)
            .With(x => x.Codes, new List<string> {"A", "B", "C"})
            .Create();
        JobCategoriesRequest? incomingCategoryTwo = _fixture.Build<JobCategoriesRequest>()
            .Without(x => x.Others)
            .With(x => x.Type, JobCategoryTypeEnum.Service)
            .With(x => x.Codes, new List<string> {"X", "Y", "Z"})
            .Create();
        List<JobCategoriesRequest> incomingCategories = new() {incomingCategoryOne, incomingCategoryTwo};

        List<BusinessJobCategory> results =
            JobCategoryService.CreateOrUpdateJobCategories(existingCategories, incomingCategories);

        Assert.NotNull(results);
        Assert.Equal(6, results.Count);
        Assert.Equal(results.Select(x => x.Code).OrderBy(x => x),
            incomingCategories.SelectMany(x => x.Codes).OrderBy(x => x));
    }

    [Fact(DisplayName =
        "Should be able to merge job categories mapping when existing mappings exist and incoming mappings are there with no other's data. Common mappings should update, extra from existing should delete.")]
    public void
        CreateOrUpdateJobCategories_WithExistingData_IncomingMappingsWithNoOthersData_ShouldDeleteExtraMappings_Success()
    {
        BusinessJobCategory? existingCategoryA = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, "A").Create();
        BusinessJobCategory? existingCategoryB = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, "B").Create();
        BusinessJobCategory? existingCategoryX = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, "X").Create();
        BusinessJobCategory? existingCategoryY = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, "Y").Create();
        List<BusinessJobCategory> existingCategories = new()
            {existingCategoryA, existingCategoryB, existingCategoryX, existingCategoryY};
        JobCategoriesRequest? incomingCategoryOne = _fixture.Build<JobCategoriesRequest>()
            .Without(x => x.Others)
            .With(x => x.Type, JobCategoryTypeEnum.Repair)
            .With(x => x.Codes, new List<string> {"A"})
            .Create();
        JobCategoriesRequest? incomingCategoryTwo = _fixture.Build<JobCategoriesRequest>()
            .Without(x => x.Others)
            .With(x => x.Type, JobCategoryTypeEnum.Service)
            .With(x => x.Codes, new List<string> {"X"})
            .Create();
        List<JobCategoriesRequest> incomingCategories = new() {incomingCategoryOne, incomingCategoryTwo};

        List<BusinessJobCategory> results =
            JobCategoryService.CreateOrUpdateJobCategories(existingCategories, incomingCategories);

        Assert.NotNull(results);
        Assert.Equal(2, results.Count);
        Assert.Equal(results.Select(x => x.Code).OrderBy(x => x),
            incomingCategories.SelectMany(x => x.Codes).OrderBy(x => x));
    }

    [Fact(DisplayName =
        "Should be able to merge job categories mapping when existing mappings exist and incoming mappings are there with no other's data. Common mappings should update, extra from existing should delete and new from incoming should add.")]
    public void
        CreateOrUpdateJobCategories_WithExistingData_IncomingMappingsWithNoOthersData_ShouldAddNewAndDeleteExtraMappings_Success()
    {
        BusinessJobCategory? existingCategoryA = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, "A").Create();
        BusinessJobCategory? existingCategoryB = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, "B").Create();
        BusinessJobCategory? existingCategoryX = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, "X").Create();
        BusinessJobCategory? existingCategoryY = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, "Y").Create();
        List<BusinessJobCategory> existingCategories = new()
            {existingCategoryA, existingCategoryB, existingCategoryX, existingCategoryY};
        JobCategoriesRequest? incomingCategoryOne = _fixture.Build<JobCategoriesRequest>()
            .Without(x => x.Others)
            .With(x => x.Type, JobCategoryTypeEnum.Repair)
            .With(x => x.Codes, new List<string> {"A", "D"})
            .Create();
        JobCategoriesRequest? incomingCategoryTwo = _fixture.Build<JobCategoriesRequest>()
            .Without(x => x.Others)
            .With(x => x.Type, JobCategoryTypeEnum.Service)
            .With(x => x.Codes, new List<string> {"X", "Z", "U"})
            .Create();
        List<JobCategoriesRequest> incomingCategories = new() {incomingCategoryOne, incomingCategoryTwo};

        List<BusinessJobCategory> results =
            JobCategoryService.CreateOrUpdateJobCategories(existingCategories, incomingCategories);

        Assert.NotNull(results);
        Assert.Equal(5, results.Count);
        Assert.Equal(results.Select(x => x.Code).OrderBy(x => x),
            incomingCategories.SelectMany(x => x.Codes).OrderBy(x => x));
    }

    [Fact(DisplayName =
        "Should be able to merge job categories mapping when existing mappings exist and incoming mappings are there with repair other's data when repair other's data does not exist. Common mappings should update, extra from existing should delete and new from incoming should add.")]
    public void
        CreateOrUpdateJobCategories_WithExistingData_IncomingMappingsWithRepairOthersData_ShouldAddNewAndDeleteExtraMappings_Success()
    {
        string repairOtherCategory = "repair_other";
        string repairOtherCategoryComments = "repair other comments 1, repair other comments 2";
        BusinessJobCategory? existingCategoryA = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, "A").Create();
        BusinessJobCategory? existingCategoryB = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, "B").Create();
        BusinessJobCategory? existingCategoryX = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, "X").Create();
        BusinessJobCategory? existingCategoryY = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, "Y").Create();
        List<BusinessJobCategory> existingCategories = new()
            {existingCategoryA, existingCategoryB, existingCategoryX, existingCategoryY};
        JobCategoriesRequest? incomingCategoryOne = _fixture.Build<JobCategoriesRequest>()
            .With(x => x.Others, repairOtherCategoryComments)
            .With(x => x.Type, JobCategoryTypeEnum.Repair)
            .With(x => x.Codes, new List<string> {"A", "D", repairOtherCategory})
            .Create();
        JobCategoriesRequest? incomingCategoryTwo = _fixture.Build<JobCategoriesRequest>()
            .Without(x => x.Others)
            .With(x => x.Type, JobCategoryTypeEnum.Service)
            .With(x => x.Codes, new List<string> {"X", "Z", "U"})
            .Create();
        List<JobCategoriesRequest> incomingCategories = new() {incomingCategoryOne, incomingCategoryTwo};

        List<BusinessJobCategory> results =
            JobCategoryService.CreateOrUpdateJobCategories(existingCategories, incomingCategories);

        Assert.NotNull(results);
        Assert.Equal(6, results.Count);
        Assert.Equal(results.Select(x => x.Code).OrderBy(x => x),
            incomingCategories.SelectMany(x => x.Codes).OrderBy(x => x));
        Assert.Equal(results.FirstOrDefault(x => x.Code == repairOtherCategory)?.Others,
            repairOtherCategoryComments);
    }

    [Fact(DisplayName =
        "Should be able to merge job categories mapping when existing mappings exist and incoming mappings are there with service other's data when service other's data does not exist. Common mappings should update, extra from existing should delete and new from incoming should add.")]
    public void
        CreateOrUpdateJobCategories_WithExistingData_IncomingMappingsWithServiceOthersData_ShouldAddNewAndDeleteExtraMappings_Success()
    {
        string serviceOtherCategory = "service_other";
        string serviceOtherCategoryComments = "service other comments 1, service other comments 2";
        BusinessJobCategory? existingCategoryA = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, "A").Create();
        BusinessJobCategory? existingCategoryB = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, "B").Create();
        BusinessJobCategory? existingCategoryX = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, "X").Create();
        BusinessJobCategory? existingCategoryY = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, "Y").Create();
        List<BusinessJobCategory> existingCategories = new()
            {existingCategoryA, existingCategoryB, existingCategoryX, existingCategoryY};
        JobCategoriesRequest? incomingCategoryOne = _fixture.Build<JobCategoriesRequest>()
            .Without(x => x.Others)
            .With(x => x.Type, JobCategoryTypeEnum.Repair)
            .With(x => x.Codes, new List<string> {"A", "D"})
            .Create();
        JobCategoriesRequest? incomingCategoryTwo = _fixture.Build<JobCategoriesRequest>()
            .With(x => x.Others, serviceOtherCategoryComments)
            .With(x => x.Type, JobCategoryTypeEnum.Service)
            .With(x => x.Codes, new List<string> {"X", "Z", serviceOtherCategory})
            .Create();
        List<JobCategoriesRequest> incomingCategories = new() {incomingCategoryOne, incomingCategoryTwo};

        List<BusinessJobCategory> results =
            JobCategoryService.CreateOrUpdateJobCategories(existingCategories, incomingCategories);

        Assert.NotNull(results);
        Assert.Equal(5, results.Count);
        Assert.Equal(results.Select(x => x.Code).OrderBy(x => x),
            incomingCategories.SelectMany(x => x.Codes).OrderBy(x => x));
        Assert.Equal(results.FirstOrDefault(x => x.Code == serviceOtherCategory)?.Others,
            serviceOtherCategoryComments);
    }

    [Fact(DisplayName =
        "Should be able to merge job categories mapping when existing mappings exist and incoming mappings are there with repair other's data when repair other's data already exists. Common mappings should update, extra from existing should delete and new from incoming should add.")]
    public void
        CreateOrUpdateJobCategories_WithExistingMappingsAndRepairOthersData_IncomingMappingsWithRepairOthersData_ShouldAddNewAndDeleteExtraMappings_Success()
    {
        string repairOtherCategory = "repair_other";
        string oldRepairOtherCategoryComments = "repair other comments 1";
        string newRepairOtherCategoryComments = "repair other comments 1, repair other comments 2";
        BusinessJobCategory? existingCategoryA = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, "A").Create();
        BusinessJobCategory? existingCategoryB = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, "B").Create();
        BusinessJobCategory? existingCategoryX = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, "X").Create();
        BusinessJobCategory? existingCategoryForRepairOthers = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, repairOtherCategory)
            .With(x => x.Others, oldRepairOtherCategoryComments)
            .Create();
        List<BusinessJobCategory> existingCategories = new()
            {existingCategoryA, existingCategoryB, existingCategoryX, existingCategoryForRepairOthers};
        JobCategoriesRequest? incomingCategoryOne = _fixture.Build<JobCategoriesRequest>()
            .With(x => x.Others, newRepairOtherCategoryComments)
            .With(x => x.Type, JobCategoryTypeEnum.Repair)
            .With(x => x.Codes, new List<string> {"A", "D", repairOtherCategory})
            .Create();
        JobCategoriesRequest? incomingCategoryTwo = _fixture.Build<JobCategoriesRequest>()
            .Without(x => x.Others)
            .With(x => x.Type, JobCategoryTypeEnum.Service)
            .With(x => x.Codes, new List<string> {"X", "Z", "U"})
            .Create();
        List<JobCategoriesRequest> incomingCategories = new() {incomingCategoryOne, incomingCategoryTwo};

        List<BusinessJobCategory> results =
            JobCategoryService.CreateOrUpdateJobCategories(existingCategories, incomingCategories);

        Assert.NotNull(results);
        Assert.Equal(6, results.Count);
        Assert.Equal(results.Select(x => x.Code).OrderBy(x => x),
            incomingCategories.SelectMany(x => x.Codes).OrderBy(x => x));
        Assert.Equal(results.FirstOrDefault(x => x.Code == repairOtherCategory)?.Others,
            newRepairOtherCategoryComments);
    }

    [Fact(DisplayName =
        "Should be able to merge job categories mapping when existing mappings exist and incoming mappings are there with service other's data when service other's data already exists. Common mappings should update, extra from existing should delete and new from incoming should add.")]
    public void
        CreateOrUpdateJobCategories_WithExistingMappingsAndServiceOthersData_IncomingMappingsWithServiceOthersData_ShouldAddNewAndDeleteExtraMappings_Success()
    {
        string serviceOtherCategory = "service_other";
        string oldServiceOtherCategoryComments = "service other comments 1";
        string newServiceOtherCategoryComments = "service other comments 1, service other comments 2";
        BusinessJobCategory? existingCategoryA = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, "A").Create();
        BusinessJobCategory? existingCategoryB = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, "B").Create();
        BusinessJobCategory? existingCategoryX = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, "X").Create();
        BusinessJobCategory? existingCategoryForServiceOthers = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, serviceOtherCategory)
            .With(x => x.Others, oldServiceOtherCategoryComments)
            .Create();
        List<BusinessJobCategory> existingCategories = new()
            {existingCategoryA, existingCategoryB, existingCategoryX, existingCategoryForServiceOthers};
        JobCategoriesRequest? incomingCategoryOne = _fixture.Build<JobCategoriesRequest>()
            .Without(x => x.Others)
            .With(x => x.Type, JobCategoryTypeEnum.Repair)
            .With(x => x.Codes, new List<string> {"A", "D", "C"})
            .Create();
        JobCategoriesRequest? incomingCategoryTwo = _fixture.Build<JobCategoriesRequest>()
            .With(x => x.Others, newServiceOtherCategoryComments)
            .With(x => x.Type, JobCategoryTypeEnum.Service)
            .With(x => x.Codes, new List<string> {"X", "Z", serviceOtherCategory})
            .Create();
        List<JobCategoriesRequest> incomingCategories = new() {incomingCategoryOne, incomingCategoryTwo};

        List<BusinessJobCategory> results =
            JobCategoryService.CreateOrUpdateJobCategories(existingCategories, incomingCategories);

        Assert.NotNull(results);
        Assert.Equal(6, results.Count);
        Assert.Equal(results.Select(x => x.Code).OrderBy(x => x),
            incomingCategories.SelectMany(x => x.Codes).OrderBy(x => x));
        Assert.Equal(results.FirstOrDefault(x => x.Code == serviceOtherCategory)?.Others,
            newServiceOtherCategoryComments);
    }

    [Fact(DisplayName =
        "Should be able to merge job categories mapping when existing mappings exist and incoming mappings are there with both repair/service other's data when both repair/service other's data already exists. Common mappings should update, extra from existing should delete and new from incoming should add.")]
    public void
        CreateOrUpdateJobCategories_WithExistingMappingsAndBothRepairServiceOthersData_IncomingMappingsWithBothRepairServiceOthersData_ShouldAddNewAndDeleteExtraMappings_Success()
    {
        string serviceOtherCategory = "service_other";
        string oldServiceOtherCategoryComments = "service other comments 1";
        string newServiceOtherCategoryComments = "service other comments 1, service other comments 2";
        string repairOtherCategory = "repair_other";
        string oldRepairOtherCategoryComments = "repair other comments 1";
        string newRepairOtherCategoryComments = "repair other comments 1, repair other comments 2";

        BusinessJobCategory? existingCategoryA = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, "A").Create();
        BusinessJobCategory? existingCategoryB = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, "B").Create();
        BusinessJobCategory? existingCategoryX = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, "X").Create();
        BusinessJobCategory? existingCategoryForServiceOthers = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, serviceOtherCategory)
            .With(x => x.Others, oldServiceOtherCategoryComments)
            .Create();
        BusinessJobCategory? existingCategoryForRepairOthers = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, repairOtherCategory)
            .With(x => x.Others, oldRepairOtherCategoryComments)
            .Create();
        List<BusinessJobCategory> existingCategories = new()
        {
            existingCategoryA, existingCategoryB, existingCategoryX, existingCategoryForServiceOthers,
            existingCategoryForRepairOthers
        };
        JobCategoriesRequest? incomingCategoryOne = _fixture.Build<JobCategoriesRequest>()
            .With(x => x.Others, newRepairOtherCategoryComments)
            .With(x => x.Type, JobCategoryTypeEnum.Repair)
            .With(x => x.Codes, new List<string> {"A", "D", repairOtherCategory})
            .Create();
        JobCategoriesRequest? incomingCategoryTwo = _fixture.Build<JobCategoriesRequest>()
            .With(x => x.Others, newServiceOtherCategoryComments)
            .With(x => x.Type, JobCategoryTypeEnum.Service)
            .With(x => x.Codes, new List<string> {"X", "Z", serviceOtherCategory})
            .Create();
        List<JobCategoriesRequest> incomingCategories = new() {incomingCategoryOne, incomingCategoryTwo};

        List<BusinessJobCategory> results =
            JobCategoryService.CreateOrUpdateJobCategories(existingCategories, incomingCategories);

        Assert.NotNull(results);
        Assert.Equal(6, results.Count);
        Assert.Equal(results.Select(x => x.Code).OrderBy(x => x),
            incomingCategories.SelectMany(x => x.Codes).OrderBy(x => x));
        Assert.Equal(results.FirstOrDefault(x => x.Code == serviceOtherCategory)?.Others,
            newServiceOtherCategoryComments);
        Assert.Equal(results.FirstOrDefault(x => x.Code == repairOtherCategory)?.Others,
            newRepairOtherCategoryComments);
    }

    [Fact(DisplayName =
        "Should be able to merge job categories mapping when existing mappings exist and incoming mappings are there with only repair other's data when both repair/service other's data already exists. Common mappings should update, extra from existing should delete and new from incoming should add.")]
    public void
        CreateOrUpdateJobCategories_WithExistingMappingsAndBothRepairServiceOthersData_IncomingMappingsWithOnlyRepairOthersData_ShouldAddNewAndDeleteExtraMappings_Success()
    {
        string serviceOtherCategory = "service_other";
        string oldServiceOtherCategoryComments = "service other comments 1";

        string repairOtherCategory = "repair_other";
        string oldRepairOtherCategoryComments = "repair other comments 1";
        string newRepairOtherCategoryComments = "repair other comments 1, repair other comments 2";

        BusinessJobCategory? existingCategoryA = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, "A").Create();
        BusinessJobCategory? existingCategoryB = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, "B").Create();
        BusinessJobCategory? existingCategoryX = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, "X").Create();
        BusinessJobCategory? existingCategoryForServiceOthers = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, serviceOtherCategory)
            .With(x => x.Others, oldServiceOtherCategoryComments)
            .Create();
        BusinessJobCategory? existingCategoryForRepairOthers = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, repairOtherCategory)
            .With(x => x.Others, oldRepairOtherCategoryComments)
            .Create();
        List<BusinessJobCategory> existingCategories = new()
        {
            existingCategoryA, existingCategoryB, existingCategoryX, existingCategoryForServiceOthers,
            existingCategoryForRepairOthers
        };
        JobCategoriesRequest? incomingCategoryOne = _fixture.Build<JobCategoriesRequest>()
            .With(x => x.Others, newRepairOtherCategoryComments)
            .With(x => x.Type, JobCategoryTypeEnum.Repair)
            .With(x => x.Codes, new List<string> {"A", "D", repairOtherCategory})
            .Create();
        JobCategoriesRequest? incomingCategoryTwo = _fixture.Build<JobCategoriesRequest>()
            .Without(x => x.Others)
            .With(x => x.Type, JobCategoryTypeEnum.Service)
            .With(x => x.Codes, new List<string> {"X", "Z"})
            .Create();
        List<JobCategoriesRequest> incomingCategories = new() {incomingCategoryOne, incomingCategoryTwo};

        List<BusinessJobCategory> results =
            JobCategoryService.CreateOrUpdateJobCategories(existingCategories, incomingCategories);

        Assert.NotNull(results);
        Assert.Equal(5, results.Count);
        Assert.Equal(results.Select(x => x.Code).OrderBy(x => x),
            incomingCategories.SelectMany(x => x.Codes).OrderBy(x => x));
        Assert.False(results.Exists(x => x.Code == serviceOtherCategory));
        Assert.Equal(results.FirstOrDefault(x => x.Code == repairOtherCategory)?.Others,
            newRepairOtherCategoryComments);
    }


    [Fact(DisplayName =
        "Should be able to merge job categories mapping when existing mappings exist and incoming mappings are there with only service other's data when both repair/service other's data already exists. Common mappings should update, extra from existing should delete and new from incoming should add.")]
    public void
        CreateOrUpdateJobCategories_WithExistingMappingsAndBothRepairServiceOthersData_IncomingMappingsWithOnlyServiceOthersData_ShouldAddNewAndDeleteExtraMappings_Success()
    {
        string serviceOtherCategory = "service_other";
        string oldServiceOtherCategoryComments = "service other comments 1";
        string newServiceOtherCategoryComments = "service other comments 1, service other comments 2";

        string repairOtherCategory = "repair_other";
        string oldRepairOtherCategoryComments = "repair other comments 1";

        BusinessJobCategory? existingCategoryA = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, "A").Create();
        BusinessJobCategory? existingCategoryB = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, "B").Create();
        BusinessJobCategory? existingCategoryX = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, "X").Create();
        BusinessJobCategory? existingCategoryForServiceOthers = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, serviceOtherCategory)
            .With(x => x.Others, oldServiceOtherCategoryComments)
            .Create();
        BusinessJobCategory? existingCategoryForRepairOthers = _fixture.Build<BusinessJobCategory>()
            .With(x => x.Code, repairOtherCategory)
            .With(x => x.Others, oldRepairOtherCategoryComments)
            .Create();
        List<BusinessJobCategory> existingCategories = new()
        {
            existingCategoryA, existingCategoryB, existingCategoryX, existingCategoryForServiceOthers,
            existingCategoryForRepairOthers
        };
        JobCategoriesRequest? incomingCategoryOne = _fixture.Build<JobCategoriesRequest>()
            .Without(x => x.Others)
            .With(x => x.Type, JobCategoryTypeEnum.Repair)
            .With(x => x.Codes, new List<string> {"A", "D"})
            .Create();
        JobCategoriesRequest? incomingCategoryTwo = _fixture.Build<JobCategoriesRequest>()
            .With(x => x.Others, newServiceOtherCategoryComments)
            .With(x => x.Type, JobCategoryTypeEnum.Service)
            .With(x => x.Codes, new List<string> {"X", "Z", serviceOtherCategory})
            .Create();
        List<JobCategoriesRequest> incomingCategories = new() {incomingCategoryOne, incomingCategoryTwo};

        List<BusinessJobCategory> results =
            JobCategoryService.CreateOrUpdateJobCategories(existingCategories, incomingCategories);

        Assert.NotNull(results);
        Assert.Equal(5, results.Count);
        Assert.Equal(results.Select(x => x.Code).OrderBy(x => x),
            incomingCategories.SelectMany(x => x.Codes).OrderBy(x => x));
        Assert.False(results.Exists(x => x.Code == repairOtherCategory));
        Assert.Equal(results.FirstOrDefault(x => x.Code == serviceOtherCategory)?.Others,
            newServiceOtherCategoryComments);
    }
}
