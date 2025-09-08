using SmartCamera.WebApiDemo.Models;
using System.ComponentModel.DataAnnotations;

namespace SmartCamera.WebApiDemo.Models
{
    public enum EventType
    {
        MotionDetected = 1,
        FaceRecognized = 2,
        UnknownFace = 3,
        ObjectDetected = 4,
        CameraOffline = 5,
        CameraOnline = 6
    }

    public enum EventSeverity
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }

    public class Event
    {
        public int Id { get; set; }

        [Required]
        public int CameraId { get; set; }

        public EventType Type { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public EventSeverity Severity { get; set; } = EventSeverity.Low;

        [StringLength(500)]
        public string? ImagePath { get; set; }

        [StringLength(500)]
        public string? VideoPath { get; set; }

        public string? Metadata { get; set; } // JSON từ AI

        public bool IsProcessed { get; set; } = false;
        public bool IsAcknowledged { get; set; } = false;

        [StringLength(100)]
        public string? AcknowledgedBy { get; set; }

        public DateTime? AcknowledgedAt { get; set; }

        // Navigation properties
        public virtual Camera Camera { get; set; } = null!;
    }
}
