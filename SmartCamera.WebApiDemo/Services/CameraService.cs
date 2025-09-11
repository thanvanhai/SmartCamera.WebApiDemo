using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartCamera.WebApiDemo.Data;
using SmartCamera.WebApiDemo.DTOs;
using SmartCamera.WebApiDemo.Models;
using SmartCamera.WebApiDemo.Services;
using SmartCamera.WebApiDemo.Messaging; // Namespace chứa IMessageProducer
using System.Net.NetworkInformation;

namespace SmartCamera.WebApi.Services
{
    public class CameraService : ICameraService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CameraService> _logger;
        private readonly IMessageProducer _producer;

        public CameraService(
            AppDbContext context,
            IMapper mapper,
            ILogger<CameraService> logger,
            IMessageProducer producer)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _producer = producer;
        }

        public async Task<IEnumerable<CameraDto>> GetAllCamerasAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var cameras = await _context.Cameras
                    .OrderBy(c => c.Name)
                    .ToListAsync(cancellationToken);

                return _mapper.Map<IEnumerable<CameraDto>>(cameras);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all cameras");
                throw;
            }
        }

        public async Task<CameraDto?> GetCameraByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var camera = await _context.Cameras.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
                return camera != null ? _mapper.Map<CameraDto>(camera) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting camera with id {Id}", id);
                throw;
            }
        }

        public async Task<CameraDto> CreateCameraAsync(CreateCameraRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var existingCamera = await _context.Cameras
                    .FirstOrDefaultAsync(c => c.IpAddress == request.IpAddress, cancellationToken);

                if (existingCamera != null)
                    throw new InvalidOperationException($"Camera with IP {request.IpAddress} already exists");

                var camera = new Camera
                {
                    Name = request.Name,
                    IpAddress = request.IpAddress,
                    Port = request.Port,
                    Username = request.Username,
                    Password = request.Password,
                    Location = request.Location,
                    Description = request.Description,
                    Type = request.Type,
                    // Nếu request.StreamUrl có thì lấy, không thì tự generate
                    StreamUrl = string.IsNullOrWhiteSpace(request.StreamUrl)
                    ? GenerateStreamUrl(request.IpAddress, request.Port, request.Username, request.Password)
                    : request.StreamUrl,
                    Status = CameraStatus.Offline,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Cameras.Add(camera);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Created new camera: {Name} with IP: {IP}", request.Name, request.IpAddress);

                // Publish event camera.registered
                await _producer.PublishAsync("smartcamera", "camera.registered", new
                {
                    id = camera.Id,
                    name = camera.Name,
                    rtsp_url = camera.StreamUrl,
                    location = camera.Location,
                    created_at = camera.CreatedAt
                }, cancellationToken);

                return _mapper.Map<CameraDto>(camera);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating camera");
                throw;
            }
        }

        public async Task<CameraDto?> UpdateCameraAsync(int id, UpdateCameraRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var camera = await _context.Cameras.FindAsync(new object?[] { id }, cancellationToken);
                if (camera == null) return null;

                camera.Name = request.Name;
                camera.Location = request.Location;
                camera.Description = request.Description;
                camera.IsActive = request.IsActive;
                camera.RecordingEnabled = request.RecordingEnabled;
                camera.MotionDetectionEnabled = request.MotionDetectionEnabled;
                camera.FaceRecognitionEnabled = request.FaceRecognitionEnabled;
                camera.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Updated camera: {Name} (ID: {Id})", camera.Name, id);

                // Publish event camera.updated
                await _producer.PublishAsync("smartcamera", "camera.updated", new
                {
                    id = camera.Id,
                    name = camera.Name,
                    location = camera.Location,
                    updated_at = camera.UpdatedAt
                }, cancellationToken);

                return _mapper.Map<CameraDto>(camera);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating camera with id {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteCameraAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var camera = await _context.Cameras.FindAsync(new object?[] { id }, cancellationToken);
                if (camera == null) return false;

                _context.Cameras.Remove(camera);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Deleted camera: {Name} (ID: {Id})", camera.Name, id);

                // Publish event camera.deleted
                await _producer.PublishAsync("smartcamera", "camera.deleted", new
                {
                    id = id,
                    deleted_at = DateTime.UtcNow
                }, cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting camera with id {Id}", id);
                throw;
            }
        }

        public async Task<bool> TestCameraConnectionAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var camera = await _context.Cameras.FindAsync(new object?[] { id }, cancellationToken);
                if (camera == null) return false;

                using var ping = new Ping();
                var reply = await ping.SendPingAsync(camera.IpAddress, 5000);
                var isOnline = reply.Status == IPStatus.Success;

                camera.Status = isOnline ? CameraStatus.Online : CameraStatus.Offline;
                camera.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Camera {Name} connection test: {Status}",
                    camera.Name, isOnline ? "Success" : "Failed");

                // Publish event camera.status.updated
                await _producer.PublishAsync("smartcamera", "camera.status.updated", new
                {
                    id = camera.Id,
                    status = camera.Status.ToString().ToLower(),
                    updated_at = camera.UpdatedAt
                }, cancellationToken);

                return isOnline;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing camera connection for id {Id}", id);
                return false;
            }
        }

        public async Task<string> GetStreamUrlAsync(int id, string quality = "HD", CancellationToken cancellationToken = default)
        {
            try
            {
                var camera = await _context.Cameras.FindAsync(new object?[] { id }, cancellationToken);
                if (camera == null) return string.Empty;

                var baseUrl = camera.StreamUrl;
                return quality.ToUpper() switch
                {
                    "SD" => $"{baseUrl}?resolution=640x480",
                    "HD" => $"{baseUrl}?resolution=1280x720",
                    "FHD" => $"{baseUrl}?resolution=1920x1080",
                    _ => baseUrl
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stream URL for camera id {Id}", id);
                return string.Empty;
            }
        }

        public async Task<IEnumerable<CameraDto>> GetCamerasByLocationAsync(string location, CancellationToken cancellationToken = default)
        {
            try
            {
                var cameras = await _context.Cameras
                    .Where(c => c.Location.Contains(location) && c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToListAsync(cancellationToken);

                return _mapper.Map<IEnumerable<CameraDto>>(cameras);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cameras by location: {Location}", location);
                throw;
            }
        }

        public async Task<bool> UpdateCameraStatusAsync(int id, CameraStatus status, CancellationToken cancellationToken = default)
        {
            try
            {
                var camera = await _context.Cameras.FindAsync(new object?[] { id }, cancellationToken);
                if (camera == null) return false;

                camera.Status = status;
                camera.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Updated camera {Name} status to {Status}", camera.Name, status);

                // Publish event camera.status.updated
                await _producer.PublishAsync("smartcamera", "camera.status.updated", new
                {
                    id = camera.Id,
                    status = status.ToString().ToLower(),
                    updated_at = camera.UpdatedAt
                }, cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating camera status for id {Id}", id);
                return false;
            }
        }

        public async Task<Dictionary<string, object>> GetCameraStatsAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var camera = await _context.Cameras
                    .Include(c => c.Events)
                    .Include(c => c.Recordings)
                    .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

                if (camera == null)
                    return new Dictionary<string, object>();

                var today = DateTime.UtcNow.Date;
                var thisWeek = DateTime.UtcNow.AddDays(-7);

                return new Dictionary<string, object>
                {
                    { "total_events", camera.Events.Count },
                    { "events_today", camera.Events.Count(e => e.Timestamp.Date == today) },
                    { "events_this_week", camera.Events.Count(e => e.Timestamp >= thisWeek) },
                    { "total_recordings", camera.Recordings.Count },
                    { "recordings_today", camera.Recordings.Count(r => r.StartTime.Date == today) },
                    { "total_storage_gb", Math.Round(camera.Recordings.Sum(r => r.FileSizeBytes) / (1024.0 * 1024 * 1024), 2) },
                    { "status", camera.Status.ToString().ToLower() },
                    { "uptime", camera.Status == CameraStatus.Online ? "online" : "offline" }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting camera stats for id {Id}", id);
                return new Dictionary<string, object>();
            }
        }

        private static string GenerateStreamUrl(string ip, int port, string username, string password)
        {
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                return $"rtsp://{username}:{password}@{ip}:{port}/stream1";
            }
            return $"rtsp://{ip}:{port}/stream1";
        }
    }
}
