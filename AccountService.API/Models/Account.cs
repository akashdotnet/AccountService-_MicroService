using Microsoft.EntityFrameworkCore;
using PodCommonsLibrary.Core.Enums;

namespace AccountService.API.Models;

[Index(nameof(Email), IsUnique = true)]
public class Account : BaseModel
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string Email { get; set; }
    public bool IsOnboardingComplete { get; set; }
    public UserRoleEnum UserRole { get; set; }
}
