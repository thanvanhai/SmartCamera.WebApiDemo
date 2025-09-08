using SmartCamera.WebApiDemo.Models;

namespace SmartCamera.WebApiDemo.DTOs
{
    public class EventDto
    {
        public int Id { get; set; }
        public int CameraId { get; set; }
        public string CameraName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Severity { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
        public string? VideoPath { get; set; }
        public bool IsProcessed { get; set; }
        public bool IsAcknowledged { get; set; }
        public string? AcknowledgedBy { get; set; }
        public DateTime? AcknowledgedAt { get; set; }
    }

    public class AcknowledgeEventRequest
    {
        public string AcknowledgedBy { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}