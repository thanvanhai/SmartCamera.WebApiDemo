// Services/AIResultsService.cs - Service to broadcast detection results
using Microsoft.AspNetCore.SignalR;
using SmartCamera.WebApiDemo.DTOs;
using SmartCamera.WebApiDemo.Hubs;

namespace SmartCamera.WebApiDemo.Services
{
    public interface IAIResultsService
    {
        Task BroadcastDetectionResult(AIResultDto result);
        Task BroadcastAlert(AlertDto alert);
        Task BroadcastCameraStatus(string cameraId, object status);
    }

    public class AIResultsService : IAIResultsService
    {
        private readonly IHubContext<AIResultsHub> _hubContext;
        private readonly ILogger<AIResultsService> _logger;

        public AIResultsService(IHubContext<AIResultsHub> hubContext, ILogger<AIResultsService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task BroadcastDetectionResult(AIResultDto result)
        {
            try
            {
                // Send to specific camera group
                await _hubContext.Clients.Group($"Camera_{result.CameraId}")
                    .SendAsync("ReceiveDetectionResult", result);

                // Also send to all cameras group (for admins)
                await _hubContext.Clients.Group("AllCameras")
                    .SendAsync("ReceiveDetectionResult", result);

                _logger.LogDebug($"🔔 Broadcasted detection result for camera {result.CameraId} - {result.DetectionCount} detections");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error broadcasting detection result: {ex.Message}");
            }
        }

        public async Task BroadcastAlert(AlertDto alert)
        {
            try
            {
                // Send alerts to all users
                await _hubContext.Clients.Group("AllUsers")
                    .SendAsync("ReceiveAlert", alert);

                _logger.LogInformation($"🚨 Broadcasted alert: {alert.Type} from camera {alert.CameraId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error broadcasting alert: {ex.Message}");
            }
        }

        public async Task BroadcastCameraStatus(string cameraId, object status)
        {
            try
            {
                await _hubContext.Clients.Group($"Camera_{cameraId}")
                    .SendAsync("CameraStatusUpdate", new { cameraId, status });

                _logger.LogDebug($"📊 Broadcasted status update for camera {cameraId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error broadcasting camera status: {ex.Message}");
            }
        }
    }
}