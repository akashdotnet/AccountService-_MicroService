namespace AccountService.API.Models;

public class ExpertSkill : BaseModel
{
    public int Id { get; set; }
    public string Code { get; set; }
    public int ExpertId { get; set; }
    public Expert Expert { get; set; }
    public string? Others { get; set; }
}
