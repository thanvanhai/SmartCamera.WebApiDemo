using Microsoft.AspNetCore.Mvc;
using SmartCamera.WebApiDemo.DTOs;
using SmartCamera.WebApiDemo.Services;
using System.ComponentModel.DataAnnotations;

namespace SmartCamera.WebApiDemo.Controllers
{
    [ApiController]
    //[Route("api/[controller]")]
    [Route("api/cameras")]   // 👈 viết thường luôn
    public class CamerasController : ControllerBase
    {
        private readonly ICameraService _cameraService;
        private readonly ILogger<CamerasController> _logger;

        public CamerasController(ICameraService cameraService, ILogger<CamerasController> logger)
        {
            _cameraService = cameraService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách tất cả camera
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CameraDto>>> GetCameras()
        {
            try
            {
                var cameras = await _cameraService.GetAllCamerasAsync();
                return Ok(cameras);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cameras");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Lấy thông tin camera theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CameraDto>> GetCamera(int id)
        {
            try
            {
                var camera = await _cameraService.GetCameraByIdAsync(id);
                if (camera == null)
                {
                    return NotFound($"Camera with ID {id} not found");
                }

                return Ok(camera);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting camera {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Tạo camera mới
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CameraDto>> CreateCamera([FromBody] CreateCameraRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var camera = await _cameraService.CreateCameraAsync(request);
                return CreatedAtAction(nameof(GetCamera), new { id = camera.Id }, camera);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating camera");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Cập nhật thông tin camera
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<CameraDto>> UpdateCamera(int id, [FromBody] UpdateCameraRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var camera = await _cameraService.UpdateCameraAsync(id, request);
                if (camera == null)
                {
                    return NotFound($"Camera with ID {id} not found");
                }

                return Ok(camera);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating camera {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Xóa camera
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCamera(int id)
        {
            try
            {
                var result = await _cameraService.DeleteCameraAsync(id);
                if (!result)
                {
                    return NotFound($"Camera with ID {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting camera {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Test kết nối camera
        /// </summary>
        [HttpPost("{id}/test-connection")]
        public async Task<ActionResult<object>> TestConnection(int id)
        {
            try
            {
                var isConnected = await _cameraService.TestCameraConnectionAsync(id);
                return Ok(new
                {
                    cameraId = id,
                    isConnected,
                    message = isConnected ? "Camera is reachable" : "Camera is not reachable",
                    testedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing camera connection {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Lấy URL stream của camera
        /// </summary>
        [HttpGet("{id}/stream")]
        public async Task<ActionResult<object>> GetStreamUrl(int id, [FromQuery] string quality = "HD")
        {
            try
            {
                var streamUrl = await _cameraService.GetStreamUrlAsync(id, quality);
                if (string.IsNullOrEmpty(streamUrl))
                {
                    return NotFound($"Camera with ID {id} not found");
                }

                return Ok(new
                {
                    cameraId = id,
                    streamUrl,
                    quality,
                    generatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stream URL for camera {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Lấy camera theo location
        /// </summary>
        [HttpGet("by-location")]
        public async Task<ActionResult<IEnumerable<CameraDto>>> GetCamerasByLocation([FromQuery][Required] string location)
        {
            try
            {
                var cameras = await _cameraService.GetCamerasByLocationAsync(location);
                return Ok(cameras);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cameras by location: {Location}", location);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Lấy thống kê camera
        /// </summary>
        [HttpGet("{id}/stats")]
        public async Task<ActionResult<Dictionary<string, object>>> GetCameraStats(int id)
        {
            try
            {
                var stats = await _cameraService.GetCameraStatsAsync(id);
                if (stats.Count == 0)
                {
                    return NotFound($"Camera with ID {id} not found");
                }

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting camera stats {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}