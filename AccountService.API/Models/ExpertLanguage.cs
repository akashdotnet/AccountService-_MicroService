namespace AccountService.API.Models;

public class ExpertLanguage : BaseModel
{
    public int Id { get; set; }
    public string Code { get; set; }
    public int ExpertId { get; set; }
    public Expert Expert { get; set; }

    public string? Others { get; set; }
}
