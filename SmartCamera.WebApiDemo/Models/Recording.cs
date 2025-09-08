using SmartCamera.WebApiDemo.Models;
using System.ComponentModel.DataAnnotations;

namespace SmartCamera.WebApiDemo.Models
{
    public enum RecordingType
    {
        Continuous = 1,
        MotionTriggered = 2,
        Scheduled = 3,
        Manual = 4
    }

    public class Recording
    {
        public int Id { get; set; }

        [Required]
        public int CameraId { get; set; }

        [Required]
        [StringLength(200)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public long FileSizeBytes { get; set; }
        public int DurationSeconds { get; set; }
        public RecordingType Type { get; set; } = RecordingType.Continuous;

        [StringLength(500)]
        public string? ThumbnailPath { get; set; }

        public bool IsArchived { get; set; } = false;
        public DateTime? ArchivedAt { get; set; }

        // Navigation properties
        public virtual Camera Camera { get; set; } = null!;
    }
}