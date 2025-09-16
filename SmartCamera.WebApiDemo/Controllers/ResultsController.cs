using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SmartCamera.WebApiDemo.DTOs;
using SmartCamera.WebApiDemo.Hubs;

namespace SmartCamera.WebApiDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResultsController : ControllerBase
    {
        private readonly IHubContext<ResultsHub> _hubContext;
        private readonly ILogger<ResultsController> _logger;

        public ResultsController(IHubContext<ResultsHub> hubContext, ILogger<ResultsController> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <summary>
        /// Endpoint để Results Processor (Python) POST kết quả AI
        /// </summary>
        [HttpPost("ai-detection")]
        public async Task<IActionResult> ReceiveAIResult([FromBody] AIResultDto result)
        {
            try
            {
                if (string.IsNullOrEmpty(result.CameraId))
                {
                    return BadRequest("CameraId is required");
                }

                _logger.LogInformation($"Received AI result for camera {result.CameraId} with {result.DetectionCount} detections");

                // Broadcast tới tất cả clients qua SignalR
                await _hubContext.Clients.All.SendAsync("ReceiveAIResult", result);

                // Broadcast tới group của camera cụ thể
                await _hubContext.Clients.Group($"camera-{result.CameraId}").SendAsync("ReceiveCameraResult", result);

                return Ok(new { message = "AI result received and broadcasted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing AI result");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Endpoint để test (có thể xóa khi production)
        /// </summary>
        [HttpGet("test")]
        public async Task<IActionResult> TestBroadcast()
        {
            var testResult = new AIResultDto
            {
                CameraId = "test-camera-1",
                WorkerId = "test-worker",
                Timestamp = DateTime.UtcNow,
                ProcessingTimeMs = 150.5,
                DetectionCount = 2,
                Detections = new List<DetectionInfo>
                {
                    new() { ClassName = "person", Confidence = 0.85, BBox = new BoundingBox { X = 100, Y = 50, Width = 80, Height = 200 } },
                    new() { ClassName = "car", Confidence = 0.92, BBox = new BoundingBox { X = 300, Y = 150, Width = 120, Height = 80 } }
                }
            };

            await _hubContext.Clients.All.SendAsync("ReceiveAIResult", testResult);
            return Ok("Test result broadcasted");
        }
    }
}