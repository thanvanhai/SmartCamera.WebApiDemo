using Microsoft.AspNetCore.SignalR;

namespace SmartCamera.WebApiDemo.Hubs
{
    public class ResultsHub : Hub
    {
        private readonly ILogger<ResultsHub> _logger;

        public ResultsHub(ILogger<ResultsHub> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Client subscribe vào camera cụ thể
        /// </summary>
        public async Task JoinCameraGroup(string cameraId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"camera-{cameraId}");
            _logger.LogInformation($"Client {Context.ConnectionId} joined camera group: {cameraId}");
        }

        /// <summary>
        /// Client unsubscribe khỏi camera
        /// </summary>
        public async Task LeaveCameraGroup(string cameraId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"camera-{cameraId}");
            _logger.LogInformation($"Client {Context.ConnectionId} left camera group: {cameraId}");
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
