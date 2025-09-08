using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace SmartCamera.WebApiDemo.Models
{
    public enum CameraStatus
    {
        Online = 1,
        Offline = 2,
        Maintenance = 3,
        Error = 4
    }

    public enum CameraType
    {
        IP = 1,
        USB = 2,
        Analog = 3,
        PTZ = 4
    }

    public class Camera
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(45)]
        public string IpAddress { get; set; } = string.Empty;

        public int Port { get; set; } = 554;

        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [StringLength(50)]
        public string Password { get; set; } = string.Empty;

        [StringLength(500)]
        public string StreamUrl { get; set; } = string.Empty;

        public CameraStatus Status { get; set; } = CameraStatus.Offline;

        public CameraType Type { get; set; } = CameraType.IP;

        [StringLength(200)]
        public string Location { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public bool RecordingEnabled { get; set; } = true;
        public bool MotionDetectionEnabled { get; set; } = true;
        public bool FaceRecognitionEnabled { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        // Navigation properties
        public virtual ICollection<Event> Events { get; set; } = new List<Event>();
        public virtual ICollection<Recording> Recordings { get; set; } = new List<Recording>();
    }
}