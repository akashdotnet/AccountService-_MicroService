using System;
using System.ComponentModel.DataAnnotations.Schema;
using AccountService.API.Dto.CustomerServiceClient.Enums;

namespace AccountService.API.Dto.CustomerServiceClient;

public class ReminderResponse
{
    public string ReminderCode { get; set; }
    public int Id { get; set; }
    public string CustomerEmail { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsActive { get; set; }
    public bool HasCustomFrequency { get; set; }
    public ReminderFrequencyEnum? CustomFrequency { get; set; }
    [Column(TypeName = "json")] public ReminderEquipmentResponse? ReminderEquipment { get; set; }
}
