namespace SmartCamera.WebApiDemo.DTOs
{
    public class RecordingDto
    {
        public int Id { get; set; }
        public int CameraId { get; set; }
        public string CameraName { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double FileSizeMB { get; set; }
        public int DurationSeconds { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? ThumbnailPath { get; set; }
        public bool IsArchived { get; set; }
        public DateTime? ArchivedAt { get; set; }
    }
}