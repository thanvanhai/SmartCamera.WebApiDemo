// Controllers/ResultsController.cs - Updated to use SignalR
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCamera.WebApiDemo.Services;
using SmartCamera.WebApiDemo.DTOs;

namespace SmartCamera.WebApiDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResultsController : ControllerBase
    {
        private readonly IAIResultsService _aiResultsService;
        private readonly ILogger<ResultsController> _logger;

        public ResultsController(IAIResultsService aiResultsService, ILogger<ResultsController> logger)
        {
            _aiResultsService = aiResultsService;
            _logger = logger;
        }

        /// <summary>
        /// Nhận kết quả AI detection từ worker (AI service) và phát broadcast qua SignalR.
        /// </summary>
        /// <param name="result">Dữ liệu kết quả AI detection (bao gồm cameraId, số lượng detection, danh sách object nhận diện).</param>
        /// <returns>Kết quả xử lý (200 nếu thành công, 400 nếu dữ liệu không hợp lệ, 500 nếu lỗi hệ thống).</returns>
        [HttpPost("ai-detection")]
        public async Task<IActionResult> ReceiveAIDetection([FromBody] AIResultDto result)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrEmpty(result.CameraId))
                {
                    return BadRequest(new { error = "CameraId is required" });
                }

                if (result.Result == null)
                {
                    return BadRequest(new { error = "Result field is required" });
                }

                // Log the received detection
                _logger.LogInformation($"📥 Received AI detection - Camera: {result.CameraId}, Detections: {result.DetectionCount}");

                // Store in database if needed (add your database logic here)
                // await _dbContext.AIResults.AddAsync(result);
                // await _dbContext.SaveChangesAsync();

                // Broadcast to connected clients via SignalR
                await _aiResultsService.BroadcastDetectionResult(result);

                // Generate alerts if needed
                if (result.DetectionCount > 0 && result.Detections.Any(d => d.Type == "person" && d.Confidence > 0.8))
                {
                    var alert = new AlertDto
                    {
                        Id = Guid.NewGuid().ToString(),
                        CameraId = result.CameraId,
                        Type = "High Confidence Person Detection",
                        Message = $"Detected {result.DetectionCount} objects with high confidence",
                        Timestamp = DateTime.UtcNow,
                        Severity = "Medium"
                    };

                    await _aiResultsService.BroadcastAlert(alert);
                }

                return Ok(new { success = true, message = "Detection result received and broadcasted" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error processing AI detection: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy danh sách detection gần nhất của 1 camera.
        /// </summary>
        /// <param name="cameraId">ID camera cần lấy dữ liệu.</param>
        /// <param name="limit">Số lượng bản ghi tối đa muốn lấy (default = 10).</param>
        /// <returns>Danh sách detection (nếu có), hoặc mảng rỗng.</returns>
        [HttpGet("camera/{cameraId}/latest")]
        public async Task<IActionResult> GetLatestDetections(string cameraId, [FromQuery] int limit = 10)
        {
            try
            {
                // Add your database query logic here
                // var results = await _dbContext.AIResults
                //     .Where(r => r.CameraId == cameraId)
                //     .OrderByDescending(r => r.Timestamp)
                //     .Take(limit)
                //     .ToListAsync();

                // For demo, return empty array
                var results = new List<AIResultDto>();

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting latest detections: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }
}