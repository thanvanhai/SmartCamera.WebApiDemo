using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SmartCamera.WebApiDemo.Hubs
{
    [Authorize]
    public class AIResultsHub : Hub
    {
        private readonly ILogger<AIResultsHub> _logger;

        public AIResultsHub(ILogger<AIResultsHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            var userName = Context.User?.Identity?.Name ?? "Unknown";

            _logger.LogInformation($"🔗 SignalR Client connected: {userName} (ID: {Context.ConnectionId})");

            // Add to general group
            await Groups.AddToGroupAsync(Context.ConnectionId, "AllUsers");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userName = Context.User?.Identity?.Name ?? "Unknown";
            _logger.LogInformation($"🔌 SignalR Client disconnected: {userName} (ID: {Context.ConnectionId})");

            if (exception != null)
            {
                _logger.LogError($"❌ SignalR Disconnect error: {exception.Message}");
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Join camera-specific group for targeted updates
        public async Task JoinCameraGroup(string cameraId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Camera_{cameraId}");
            _logger.LogDebug($"📹 Client {Context.ConnectionId} joined camera group: {cameraId}");
        }

        public async Task LeaveCameraGroup(string cameraId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Camera_{cameraId}");
            _logger.LogDebug($"📹 Client {Context.ConnectionId} left camera group: {cameraId}");
        }

        // Join all cameras for admin users
        public async Task JoinAllCameras()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "AllCameras");
            _logger.LogDebug($"👑 Admin client {Context.ConnectionId} joined all cameras group");
        }
    }
}