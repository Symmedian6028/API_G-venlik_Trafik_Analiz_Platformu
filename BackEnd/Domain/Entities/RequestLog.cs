namespace Domain.Entities;

public class RequestLog
{
    public int Id { get; set; } // Veritabanındaki benzersiz kimlik (Primary Key)
    public string Endpoint { get; set; } = string.Empty; // Örn: "/api/status"
    public string ClientIp { get; set; } = string.Empty; // İsteği yapanın IP'si
    public int StatusCode { get; set; } // Örn: 200, 404, 500
    public long DurationMs { get; set; } // İsteğin ne kadar sürdüğü (milisaniye)
    public DateTime Timestamp { get; set; } = DateTime.UtcNow; // İsteğin geliş zamanı
}