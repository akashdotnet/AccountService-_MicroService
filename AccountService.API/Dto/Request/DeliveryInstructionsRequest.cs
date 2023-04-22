namespace AccountService.API.Dto.Request;

public class DeliveryInstructionsRequest
{
    public string? SubdivisionName { get; set; }
    public string? HomeAccessDetails { get; set; }
    public string? PetInformation { get; set; }
    public string? HealthAndSafetyInformation { get; set; }
    public string? PoolOrEquipmentNotes { get; set; }
}
