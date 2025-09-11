using SmartCamera.WebApiDemo.Models;
using System.ComponentModel.DataAnnotations;

namespace SmartCamera.WebApiDemo.DTOs
{
    public class CameraDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public int Port { get; set; }
        public string StreamUrl { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool RecordingEnabled { get; set; }
        public bool MotionDetectionEnabled { get; set; }
        public bool FaceRecognitionEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateCameraRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(45)]
        public string IpAddress { get; set; } = string.Empty;

        [Range(1, 65535)]
        public int Port { get; set; } = 554;

        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [StringLength(50)]
        public string Password { get; set; } = string.Empty;

        [StringLength(500)]
        public string? StreamUrl { get; set; }   // ✅ Cho phép user nhập thủ công

        [StringLength(200)]
        public string Location { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public CameraType Type { get; set; } = CameraType.IP;
    }

    public class UpdateCameraRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string Location { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [StringLength(500)]
        public string? StreamUrl { get; set; }   // ✅ Cho phép user nhập thủ công

        public bool IsActive { get; set; } = true;
        public bool RecordingEnabled { get; set; } = true;
        public bool MotionDetectionEnabled { get; set; } = true;
        public bool FaceRecognitionEnabled { get; set; } = false;
    }

    public class CameraStreamRequest
    {
        public int CameraId { get; set; }
        public string Quality { get; set; } = "HD"; // SD, HD, FHD
    }
}