namespace AccountService.API.Models;

public class CustomerDeliveryInstruction : BaseModel
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; }
    public string? SubdivisionName { get; set; }
    public string? HomeAccessDetails { get; set; }
    public string? PetInformation { get; set; }
    public string? HealthAndSafetyInformation { get; set; }
    public string? PoolOrEquipmentNotes { get; set; }
}
