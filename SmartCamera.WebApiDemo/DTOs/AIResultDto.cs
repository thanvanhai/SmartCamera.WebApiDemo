namespace SmartCamera.WebApiDemo.DTOs
{
    public class AIResultDto
    {
        public string CameraId { get; set; } = string.Empty;
        public string WorkerId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public double ProcessingTimeMs { get; set; }
        public int DetectionCount { get; set; }
        public List<DetectionInfo> Detections { get; set; } = new();
        public string? AnnotatedFrameBase64 { get; set; }
    }

    public class DetectionInfo
    {
        public string ClassName { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public BoundingBox BBox { get; set; } = new();
    }

    public class BoundingBox
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}