using System.Collections.Generic;
using System.IO;
using AccountService.API.Enums;
using Microsoft.AspNetCore.Http;

namespace AccountService.API.Constants;

public static class StaticValues
{
    // API Paths
    public const string ApiRoutePrefix = "api";
    public const string DealerControllerRoutePrefix = $"{ApiRoutePrefix}/dealers";
    public const string BusinessControllerRoutePrefix = $"{ApiRoutePrefix}/businesses";
    public const string ExpertControllerRoutePrefix = $"{ApiRoutePrefix}/experts";
    public const string OtpControllerRoutePrefix = $"{ApiRoutePrefix}/otp";
    public const string LogoutPath = "logout";
    public const string BusinessLogoUploadPath = "logo";
    public const string ProfilePhotoUploadPath = "profile-photo";
    public const string GeneratePath = "generate";
    public const string ValidatePath = "validate";
    public const string CustomerByEmailInternalPath = $"/{ApiRoutePrefix}/internal/customers/{{email}}";
    public const string CustomersByEmailsInternalPath = $"/{ApiRoutePrefix}/internal/customers/details";
    public const string CustomerIdInternalPath = $"/{ApiRoutePrefix}/internal/customers/id";
    public const string CustomerDeleteProfilePicturePath = $"/{ApiRoutePrefix}/customers/profile-picture";

    public const string CustomerFreeCallStatusInternalPath =
        $"/{ApiRoutePrefix}/internal/customers/free-call-status";

    public const string CustomerFreeCallStatusPath =
        $"/{ApiRoutePrefix}/customers/free-call-status";

    public const string NotificationPreferencesPath =
        $"/{ApiRoutePrefix}/customers/notifications/preferences";

    public const string ExpertByEmailInternalPath = $"/{ApiRoutePrefix}/internal/experts/{{email}}";
    public const string DealerByBusinessLocationInternalPath = "internal/dealer-profiles";
    public const string DealerProfilesPath = $"/{ApiRoutePrefix}/dealer-profiles";
    public const string CustomerProfilesByEmailInternalPath = $"/{ApiRoutePrefix}/internal/customers";
    public const string ExpertProfilesByEmailInternalPath = $"/{ApiRoutePrefix}/internal/experts";
    public const string CustomerDeliveryInstructionPath = $"/{ApiRoutePrefix}/customers/delivery-instructions";
    public const string CustomerSuggestedDealerPath = $"/{ApiRoutePrefix}/customers/invite-dealer";

    public const string DealerLocationProfilesPath =
        $"/{ApiRoutePrefix}/dealer-location-profiles/{{businessLocationId}}";

    public const string TermsAndConditionsPath = "tnc";

    // Header Params
    public const string EmailHeader = "x-user-id";
    public const string UserTypeHeader = "x-user-type";

    // Payment Service API Paths
    public const string CustomerPath = $"{ApiRoutePrefix}/internal/customer";
    public const string DealerBankAccountPath = $"{ApiRoutePrefix}/dealers/bank-account";

    // Catalog Service API Paths
    public const string JobCategoriesPath = $"{ApiRoutePrefix}/job-categories";
    public const string LanguagesPath = $"{ApiRoutePrefix}/languages";
    public const string SkillsPath = $"{ApiRoutePrefix}/skills";
    public const string BlobsPath = $"{ApiRoutePrefix}/blobs";
    public const string BrandsPath = $"{ApiRoutePrefix}/brands";
    public const string InternalDealersWorkOrdersPath = $"{ApiRoutePrefix}/internal/dealers/work-orders";
    public const string UploadFilePath = $"{ApiRoutePrefix}/blobs/upload";
    public const string StatesPath = $"{ApiRoutePrefix}/states";
    public const string CountyByZipCodePath = $"{ApiRoutePrefix}/states/counties";
    public const string PoolDetailLookupsPath = $"{ApiRoutePrefix}/pool-detail-lookups";

    // Catalog Service API Paths
    public const string CreateCustomerRemindersPath = $"{ApiRoutePrefix}/customers/internal/reminders/seed";

    // Video Call Service API Paths
    public const string AgentsPath = $"{ApiRoutePrefix}/internal/agents";
    public const string FavouriteDealerPath = $"/{ApiRoutePrefix}/customers/favourite-dealer";
    public const string DealerByMatchCriteriaInternalPath = "internal/dealer-profile-details";

    // Customer Auth API Paths
    public const string CustomersControllerRoutePrefix = $"{ApiRoutePrefix}/customers";
    public const string SignUpCustomerPath = "customers/signup";
    public const string LoginPath = "login";
    public const string RefreshTokenPath = "token/refresh";
    public const string CustomerSessionPath = "customers/session";
    public const string CustomerResetPasswordTokenPath = "customers/reset";
    public const string CustomerPasswordPath = "customers/password";
    public const string ExpertProfilesPath = "expert-profiles";
    public const string LogoutCustomerPath = "customers/logout";

    // Merchant Auth API Paths
    public const string MerchantRoutePrefix = $"{ApiRoutePrefix}/merchant";
    public const string LoginRedirectPath = "login/redirect";
    public const string LoginTokenPath = "login/token";
    public const string SfTokenPath = "services/oauth2/token";
    public const string UserInfoPath = "services/oauth2/userinfo";
    public const string SfAuthorizePath = "services/oauth2/authorize";
    public const string RevokeTokenPath = "services/oauth2/revoke";

    // Query Parameters
    public const string SfAccessTokenQueryParameter = "access_token";
    public const string BusinessLocationIdsQueryParam = "businessLocationIds";

    // Form Body Parameters
    public const string AuthCodeGrantType = "authorization_code";
    public const string RefreshTokenGrantType = "refresh_token";

    // App constants
    public const string EmailClaim = "email";
    public const string UserTypeClaim = "userType";
    public const string DealersContainerName = "dealers";
    public const string ExpertsContainerName = "experts";
    public const string CustomersContainerName = "customers";
    public const string ZipCodeRegex = @"^[0-9]{5}(?:[- ][0-9]{4})?$";
    public const int DefaultMaxLength = 250;
    public const int AboutMaxLength = 3000;
    public const int MinExperienceInYears = 1;
    public const int MaxExperienceInYears = 30;
    public const string UnknownUserName = "Unknown";
    public const string RefreshTokenKey = "REFRESH_TOKEN";
    public const string PentairEmailHostName = "pentair.com";
    private const string VerifyEmailAddressSubject = "Please verify your account";
    public const string InviteDealerSubject = "Customer Request to add a Dealer";
    private const string ResetPasswordEmailSubject = "Reset password for Ripple account";
    public const string EmailVerificationTemplateName = "OtpEmailTemplate.html";
    public const string ResetEmailTemplateName = "ResetEmailTemplate.html";
    public const string InviteDealerEmailTemplateName = "InviteDealerEmailTemplate.html";
    public const string DSTDealerSignUpEmailTemplateName = "DSTDealerSignUpEmailTemplate.html";
    public const string JobCategoryRepairOtherName = "repair_other";
    public const string JobCategoryServiceOtherName = "service_other";
    public const string OthersCode = "others";
    public const int MinDealerStartYear = 2000;
    public const int MaxServicedPoolCountForDealer = 999999;
    public const string QualifiedDealersCachePattern = "QUALIFIED_DEALERS*";
    public const string RequestHeaderForEmail = "x-user-id";
    public const string NewDealerSignUpSubject = "A new dealer has signed up";

    // Error constants
    public const string ErrorUserRegister = "User registration error!";
    public const string ErrorUserLogin = "User login error!";
    public const string ErrorInvalidRefreshToken = "Refresh token is invalid!";
    public const string ErrorRefreshTokenOTPInvalid = "Refresh token OTP not verified!";
    public const string ErrorIncorrectCredentials = "Wrong username or password!";
    public const string ErrorResetToken = "Reset token failure!";
    public const string ErrorCatalogServiceBaseAddressNotFound = "The base address for catalog service is not found!";
    public const string ErrorCustomerServiceBaseAddressNotFound = "The base address for customer service is not found!";
    public const string ErrorJobServiceBaseAddressNotFound = "The base address for job service is not found!";
    public const string ErrorPaymentServiceBaseAddressNotFound = "The base address for payment service is not found!";

    public const string ErrorVideoCallServiceBaseAddressNotFound =
        "The base address for video call service is not found!";

    public const string ErrorSalesForceBaseAddressNotFound = "The base address for SalesForce is not found!";
    public const string ErrorUserTypeNotSupported = "We only support dealer and expert user types!";
    public const string ErrorUserTypeMappingNotFound = "The mapping for the given user type doesn't exist!";

    public const string ErrorInvalidZipCode = "Invalid Zip Code!";
    public const string ErrorBusinessNotFoundForDealer = "Unable to find business for the logged in user";
    public const string ErrorCustomerNotFound = "Unable to find customer for the logged in user!";
    public const string ErrorDealerNotFound = "Unable to find dealer for the logged in user!";
    public const string ExpertNotFoundError = "Unable to find expert for the logged in user!";
    public const string ErrorRequiredBusinessName = "Business name field is required to create an business";
    public const string ErrorProfilePictureNotFound = "Unable to find profile picture for logged in user!";

    public const string ErrorInvalidPentairEmail =
        "Invalid email address. Please provide a valid pentair email address.";

    public const string ErrorInvalidNonPentairEmail =
        "Invalid email address. Please provide a valid non-pentair email address.";

    public const string ErrorIncorrectOtp = "Invalid OTP";
    public const string ErrorOtpExpired = "The OTP has expired, please generate a new OTP";

    public const string ErrorEmailNotVerifiedBeforeRegistration =
        "Email is not verified. Please verify the email before registration";

    public const string ErrorInvalidZipCodeForSelectedState = "Invalid Zip Code For Selected State!";
    public const string ErrorCustomerStateNotFound = "Unable to find primary address state for the logged in customer!";
    public const string ErrorChangePassword = "Unable to change the password!";
    public const string ErrorSameCurrentAndNewPassword = "The new password cannot be same as the old password!";
    public const string ErrorCustomerAlreadyDeleted = "The customer is already deleted!";
    public const string ErrorTermsAndConditionsNotAccepted = "User needs to agree to terms and conditions to proceed!";

    public const string InvalidJobCategoryCode = "InvalidJobCategoryCode";
    public const string InvalidLanguageCode = "InvalidLanguageCode";
    public const string InvalidSkillCode = "InvalidSkillCode";
    public const string InvalidWebSiteUrl = "InvalidWebSiteUrl";
    public const string ExistingWorkOrder = "InvalidWebSiteUrl";

    // Errors
    public const string CatalogServiceBaseAddressNotFound = "CatalogServiceBaseAddressNotFound";
    public const string CustomerServiceBaseAddressNotFound = "CustomerServiceBaseAddressNotFound";
    public const string VideoCallServiceBaseAddressNotFound = "VideoCallServiceBaseAddressNotFound";
    public const string SalesForceBaseAddressNotFound = "SalesForceBaseAddressNotFound";
    public const string PaymentServiceBaseAddressNotFound = "PaymentServiceBaseAddressNotFound";
    public const string RegistrationFailed = "RegistrationFailed";
    public const string ServiceUnavailable = "ServiceUnavailable";
    public const string UserDoesNotExist = "UserDoesNotExist";
    public const string CustomerNotFound = "CustomerNotFound";
    public const string DealerNotFound = "DealerNotFound";
    public const string ExpertNotFound = "ExpertNotFound";
    public const string LocationNotFound = "LocationNotFound";
    public const string ProfilePictureNotFound = "ProfilePictureNotFound";
    public const string InvalidExperienceInYears = "InvalidExperienceInYears";
    public const string InvalidStartYear = "InvalidExperienceInYears";
    public const string BusinessNotFound = "BusinessNotFound";
    public const string UserWithSameEmailExists = "UserWithSameEmailExists";
    public const string InvalidBrandCode = "InvalidBrandCode";
    public const string InvalidStateName = "InvalidStateName";
    public const string InvalidCounties = "InvalidCounties";
    public const string InvalidZipCode = "InvalidZipCode";
    public const string RequiredBusinessName = "RequiredBusinessName";
    public const string InvalidPoolSizeCode = "InvalidPoolSizeCode";
    public const string InvalidPoolTypeCode = "InvalidPoolTypeCode";
    public const string InvalidSanitationMethodCode = "InvalidSanitationMethodCode";
    public const string InvalidPoolSeasonCode = "InvalidPoolSeasonCode";
    public const string InvalidHotTubTypeCode = "InvalidHotTubTypeCode";
    public const string InvalidPoolMaterialCode = "InvalidPoolMaterialCode";
    public const string InvalidSession = "InvalidSession";
    public const string OtpCoolDownError = "OtpCoolDownError";
    public const string InvalidOtp = "InvalidOtp";
    public const string OtpExpired = "OtpExpired";
    public const string EmailNotVerified = "EmailNotVerified";
    public const string InvalidUserType = "InvalidUserType";
    public const string InvalidZipCodeForSelectedState = "InvalidZipCodeForSelectedState";
    public const string BusinessLocationNotFound = "BusinessLocationNotFound";
    public const string CustomerStateNotFound = "CustomerStateNotFound";
    public const string HtmlTemplateParametersNotFound = "HtmlTemplateParametersNotFound";
    public const string ChangedPasswordFailed = "ChangedPasswordFailed";
    public const string CustomerAlreadyDeleted = "CustomerAlreadyDeleted";
    public const string TermsAndConditionsNotAccepted = "TermsAndConditionsNotAccepted";


    public static readonly string MinDealerStartYearOptionValue = $"Before {MinDealerStartYear}";

    public static readonly Dictionary<EmailTemplateEnum, Dictionary<string, string>> EmailTemplateConfig = new()
    {
        {
            EmailTemplateEnum.Register, new Dictionary<string, string>
            {
                {"Template", EmailVerificationTemplateName},
                {"Subject", VerifyEmailAddressSubject}
            }
        },
        {
            EmailTemplateEnum.Reset, new Dictionary<string, string>
            {
                {"Template", ResetEmailTemplateName},
                {"Subject", ResetPasswordEmailSubject}
            }
        }
    };

    public static string GetBusinessLogoFileName(IFormFile logo, int dealerId)
    {
        //creates a folder with dealer id as the name and adds Business_Logo inside it
        return $"{dealerId}/Business_Logo{Path.GetExtension(logo.FileName)}";
    }

    public static string GetExpertProfilePhotoFileName(IFormFile logo, int expertId)
    {
        //creates a folder with expert id as the name and adds Profile_Photo inside it
        return $"{expertId}/Profile_Photo{Path.GetExtension(logo.FileName)}";
    }

    public static string GetCustomerProfilePhotoFileName(IFormFile profilePhoto, int customerId)
    {
        //creates a folder with expert id as the name and adds Profile_Photo inside it
        return $"{customerId}/Profile_Photo{Path.GetExtension(profilePhoto.FileName)}";
    }

    public static string GetBlobByBlobIdUrlPath(int blobId)
    {
        return $"{ApiRoutePrefix}/blobs/{blobId}";
    }

    public static string DeleteBlobByBlobIdUrlPath(int blobId)
    {
        return $"{ApiRoutePrefix}/blobs/{blobId}";
    }

    public static string GetBlobsByBlobIdsUrlPath()
    {
        return $"{ApiRoutePrefix}/blobs";
    }

    public static string ErrorBusinessLocationNotFound(int businessLocationId)
    {
        return $"Unable to find the business location with id: {businessLocationId}";
    }

    public static string ErrorLocationNotFound(int id, int businessId)
    {
        return $"Unable to find location with id: {id} the business: {businessId}";
    }

    public static string ErrorInvalidState(string invalidState, string[] validStates)
    {
        return $"{invalidState} is not a valid state. Allowed states are: {string.Join(",", validStates)}";
    }

    public static string ErrorInvalidCounties(List<string> invalidCounties, List<string> validCounties, string state)
    {
        return
            $"{string.Join(",", invalidCounties)} are not valid counties. For state: {state} allowed counties are: {string.Join(",", validCounties)}";
    }

    public static string ErrorInvalidZipCodeForState(string invalidZipCode, List<string> validZipCodes)
    {
        return
            $"{invalidZipCode} is not a valid zip code for the selected state. Valid Zip codes are: {string.Join(",", validZipCodes)}";
    }

    public static string ErrorInvalidBrandCode(string brandCode, string[] allowedBrandCodes)
    {
        return $"{brandCode} is not a valid brand code. Allowed brand codes are: {string.Join(",", allowedBrandCodes)}";
    }

    public static string ErrorInvalidStartYear(int currentYear, string startYear)
    {
        return
            $"{startYear} is not a valid start year. Valid values are {MinDealerStartYear}-{currentYear} or {MinDealerStartYearOptionValue}";
    }

    public static string ExistingWorkOrderErrorMessage(List<int> businessLocationIds)
    {
        return
            $"The following business locations with ids: {string.Join(",", businessLocationIds)} cannot be deleted as there are existing work orders associated with them.";
    }

    public static string ErrorInvalidExperienceInYears()
    {
        return
            $"The years of experience should have value between {MinExperienceInYears}-{MaxExperienceInYears} or {MaxExperienceInYears}+";
    }

    public static string ErrorInvalidSession(string sessionId)
    {
        return $"Unable to find session with id: {sessionId}";
    }

    public static string ErrorOtpCoolDown(double durationInSeconds)
    {
        return $"An OTP has already been sent, new OTP can be sent only after {durationInSeconds} seconds";
    }
}
