using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountService.API.Clients.Interfaces;
using AccountService.API.Constants;
using AccountService.API.Dto.CatalogServiceClient;
using AccountService.API.Dto.Request;
using AccountService.API.Dto.Response;
using AccountService.API.Dto.SalesForce;
using AccountService.API.Dto.VideoCallServiceClient;
using AccountService.API.Enums;
using AccountService.API.Models;
using AccountService.API.Repositories.Interfaces;
using AccountService.API.Services.Interfaces;
using AutoMapper;
using PodCommonsLibrary.Core.Enums;
using PodCommonsLibrary.Core.Exceptions;

namespace AccountService.API.Services;

public class ExpertService : IExpertService
{
    private readonly IAccountService _accountService;
    private readonly ICatalogServiceClient _catalogServiceClient;
    private readonly IExpertRepository _expertRepository;
    private readonly IMapper _mapper;
    private readonly IVideoCallServiceClient _videoCallServiceClient;

    public ExpertService(
        IExpertRepository expertRepository,
        IMapper mapper,
        ICatalogServiceClient catalogServiceClient,
        IAccountService accountService,
        IVideoCallServiceClient videoCallServiceClient
    )
    {
        _expertRepository = expertRepository;
        _mapper = mapper;
        _catalogServiceClient = catalogServiceClient;
        _accountService = accountService;
        _videoCallServiceClient = videoCallServiceClient;
    }

    public async Task<Expert> UpdateExpert(UpdateExpertRequest updateExpertRequest)
    {
        Expert expert = await GetExpert();

        expert.JoiningYear = updateExpertRequest.ExperienceInYears != null
            ? GetJoiningYearFromYearsOfExperience(updateExpertRequest.ExperienceInYears)
            : expert.JoiningYear;

        expert.Languages = updateExpertRequest.Languages != null
            ? LanguageService.CreateOrUpdateExpertLanguages(expert.Languages, updateExpertRequest.Languages)
            : expert.Languages;

        expert.Skills = updateExpertRequest.Skills != null
            ? SkillService.CreateOrUpdateExpertSkills(expert.Skills, updateExpertRequest.Skills)
            : expert.Skills;

        expert.ZipCode = updateExpertRequest.ZipCode ?? expert.ZipCode;
        expert.About = updateExpertRequest.About ?? expert.About;
        ExpertStepEnum currentExpertStep =
            (ExpertStepEnum) Enum.Parse(typeof(ExpertStepEnum), updateExpertRequest.OnboardingStep.ToString());
        if (currentExpertStep > expert.LastCompletedOnboardingStep)
        {
            expert.LastCompletedOnboardingStep = currentExpertStep;
        }

        expert.Account.IsOnboardingComplete =
            expert.LastCompletedOnboardingStep == ExpertStepEnum.ExpertProfileCompletion;
        if (expert.Account.IsOnboardingComplete && expert.AgentId == null)
        {
            AgentResponse agentResponse = await CreateAgent(expert);
            expert.AgentId = agentResponse.Id;
        }

        return await _expertRepository.UpdateExpert(expert);
    }

    public async Task<ExpertResponse> CreateExpert(UserInformationResponse userInformation)
    {
        Account account = _mapper.Map<Account>(userInformation);
        account.UserRole = UserRoleEnum.Expert;
        Expert expert = new()
        {
            Account = account,
            LastCompletedOnboardingStep = ExpertStepEnum.SignUpComplete
        };
        Expert createdExpert = await _expertRepository.CreateExpert(expert);
        return _mapper.Map<ExpertResponse>(createdExpert);
    }

    public async Task<ExpertResponse> GetExpertWithProfilePhoto()
    {
        Expert expert = await GetExpert();

        ExpertResponse expertResponse = _mapper.Map<ExpertResponse>(expert);
        //if the business logo is previously upload
        int? profilePhotoBlobId = expert.ProfilePhotoBlobId;
        if (profilePhotoBlobId != null)
        {
            BlobResponse blobResponse = await _catalogServiceClient.GetBlobUrl((int) profilePhotoBlobId);
            expertResponse.ProfilePhotoUrl = blobResponse.BlobUrl;
        }

        return expertResponse;
    }

    public async Task<ExpertResponse> GetExpertByEmail(string email)
    {
        Expert expert = await GetExpert(email);
        return _mapper.Map<ExpertResponse>(expert);
    }

    public async Task<List<ExpertResponse>> GetExpertProfiles(ExpertByEmailRequest expertByEmailRequest)
    {
        // get all the customer details from emails
        List<Expert> experts =
            await _expertRepository.GetExpertsByEmailIds(expertByEmailRequest.Emails);

        if (!experts.Any())
        {
            throw new NotFoundException(StaticValues.ExpertNotFound,
                StaticValues.ExpertNotFoundError);
        }

        return _mapper.Map<List<ExpertResponse>>(experts);
    }

    public async Task<List<ExpertResponse>> GetAllExpertProfiles()
    {
        List<Expert> experts = await _expertRepository.GetExperts();
        IEnumerable<Task<BlobResponse>> blobTasks = experts.Select(expert =>
        {
            int? blobId = expert.ProfilePhotoBlobId;
            return blobId != null
                ? _catalogServiceClient.GetBlobUrl((int) blobId)
                : Task.FromResult(new BlobResponse());
        });
        BlobResponse[] blobUrls = await Task.WhenAll(blobTasks);
        return experts.Select((expert, index) =>
        {
            ExpertResponse expertResponse = _mapper.Map<ExpertResponse>(expert);
            expertResponse.ProfilePhotoUrl = blobUrls[index].BlobUrl;
            return expertResponse;
        }).ToList();
    }

    public async Task<ProfilePhotoUploadResponse> UploadExpertProfilePhoto(
        ProfilePhotoUploadRequest profilePhotoUploadRequest)
    {
        Expert expert = await GetExpert();

        string profilePhotoFileName =
            StaticValues.GetExpertProfilePhotoFileName(profilePhotoUploadRequest.ProfilePhoto, expert.Id);
        BlobResponse blobResponse = await _catalogServiceClient.UploadFile(
            profilePhotoUploadRequest.ProfilePhoto, profilePhotoFileName, StaticValues.ExpertsContainerName,
            expert.ProfilePhotoBlobId);
        expert.ProfilePhotoBlobId = blobResponse.BlobId;
        await _expertRepository.UpdateExpert(expert);
        return new ProfilePhotoUploadResponse
        {
            ProfilePhotoUrl = blobResponse.BlobUrl
        };
    }

    private async Task<AgentResponse> CreateAgent(Expert expert)
    {
        CreateAgentRequest createAgentRequest = new()
        {
            Email = expert.Account.Email,
            PhoneNumber = expert.Account.PhoneNumber,
            Name = $"{expert.Account.FirstName} {expert.Account.LastName}"
        };
        return await _videoCallServiceClient.CreateAgent(createAgentRequest);
    }

    private async Task<Expert> GetExpert(string? email = null)
    {
        int accountId = await _accountService.GetAccountId(email);
        Expert? expert = await _expertRepository.GetExpertByAccountId(accountId);
        if (expert == null)
        {
            throw new NotFoundException(StaticValues.ExpertNotFound,
                StaticValues.ExpertNotFoundError);
        }

        return expert;
    }

    private static int GetJoiningYearFromYearsOfExperience(string yearsOfExperience)
    {
        int currentYear = DateTime.UtcNow.Year;
        if (int.TryParse(yearsOfExperience, out int yearsOfExperienceAsInteger))
        {
            if (yearsOfExperienceAsInteger >= StaticValues.MinExperienceInYears &&
                yearsOfExperienceAsInteger <= StaticValues.MaxExperienceInYears)
            {
                return currentYear - yearsOfExperienceAsInteger;
            }
        }
        else if (yearsOfExperience == $"{StaticValues.MaxExperienceInYears}+")
        {
            return currentYear - (StaticValues.MaxExperienceInYears + 1);
        }

        throw new BusinessRuleViolationException(StaticValues.InvalidExperienceInYears,
            StaticValues.ErrorInvalidExperienceInYears());
    }

    public static string? GetYearsOfExperienceFromJoiningYear(int? joiningYear)
    {
        if (joiningYear is null)
        {
            return null;
        }

        int currentYear = DateTime.UtcNow.Year;
        int yearsOfExperience = currentYear - (int) joiningYear;
        return yearsOfExperience <= StaticValues.MaxExperienceInYears
            ? yearsOfExperience.ToString()
            : $"{StaticValues.MaxExperienceInYears}+";
    }
}
