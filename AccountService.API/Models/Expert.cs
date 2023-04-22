using System.Collections.Generic;
using AccountService.API.Enums;
using Microsoft.EntityFrameworkCore;

namespace AccountService.API.Models;

[Index("AccountId", IsUnique = true)]
public class Expert : BaseModel
{
    public int Id { get; set; }
    public string? AgentId { get; set; }
    public string? ZipCode { get; set; }
    public int? ProfilePhotoBlobId { get; set; }
    public int? JoiningYear { get; set; }
    public string? About { get; set; }
    public int AccountId { get; set; }
    public Account Account { get; set; }
    public ExpertStepEnum LastCompletedOnboardingStep { get; set; }
    public List<ExpertSkill> Skills { get; set; } = new();
    public List<ExpertLanguage> Languages { get; set; } = new();
}
