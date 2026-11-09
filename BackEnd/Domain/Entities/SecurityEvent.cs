namespace Domain.Entities;

public class SecurityEvent
{
    public int Id { get; set; } 
    public string EventType { get; set; } = string.Empty; 
    public string Description { get; set; } = string.Empty; 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
}