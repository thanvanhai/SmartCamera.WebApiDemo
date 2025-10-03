// DTOs/AIResultDto.cs - Data Transfer Objects
namespace SmartCamera.WebApiDemo.DTOs
{
    public class AIResultDto
    {
        public string CameraId { get; set; } = string.Empty;
        public string WorkerId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public double ProcessingTimeMs { get; set; }
        public int DetectionCount { get; set; }
        public List<DetectionDto> Detections { get; set; } = new();
        public object Result { get; set; } = new(); // This was the missing field causing 400 error
    }

    public class DetectionDto
    {
        public string? Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public BoundingBoxDto BoundingBox { get; set; } = new();
    }

    public class BoundingBoxDto
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class AlertDto
    {
        public string Id { get; set; } = string.Empty;
        public string CameraId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Severity { get; set; } = "Low"; // Low, Medium, High, Critical
    }
}