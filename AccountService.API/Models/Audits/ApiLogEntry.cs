using System;

namespace AccountService.API.Models.Audits;

public class ApiLogEntry
{
    public int Id { get; set; }
    public string JsonData { get; set; }
    public DateTime InsertedOn { get; set; }
    public string EventType { get; set; }
    public string ResponseStatus { get; set; }
    public string ResponseStatusCode { get; set; }
    public string UserName { get; set; }
}
