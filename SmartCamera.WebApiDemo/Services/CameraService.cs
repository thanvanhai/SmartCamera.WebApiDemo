using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartCamera.WebApiDemo.Data;
using SmartCamera.WebApiDemo.DTOs;
using SmartCamera.WebApiDemo.Models;
using SmartCamera.WebApiDemo.Services;
using System.Net.NetworkInformation;

namespace SmartCamera.WebApi.Services
{
    public class CameraService : ICameraService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CameraService> _logger;

        public CameraService(AppDbContext context, IMapper mapper, ILogger<CameraService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<CameraDto>> GetAllCamerasAsync()
        {
            try
            {
                var cameras = await _context.Cameras
                    .Where(c => !c.IsActive || c.IsActive) // Có thể filter theo IsActive
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<CameraDto>>(cameras);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all cameras");
                throw;
            }
        }

        public async Task<CameraDto?> GetCameraByIdAsync(int id)
        {
            try
            {
                var camera = await _context.Cameras
                    .FirstOrDefaultAsync(c => c.Id == id);

                return camera != null ? _mapper.Map<CameraDto>(camera) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting camera with id {Id}", id);
                throw;
            }
        }

        public async Task<CameraDto> CreateCameraAsync(CreateCameraRequest request)
        {
            try
            {
                // Check if IP already exists
                var existingCamera = await _context.Cameras
                    .FirstOrDefaultAsync(c => c.IpAddress == request.IpAddress);

                if (existingCamera != null)
                {
                    throw new InvalidOperationException($"Camera with IP {request.IpAddress} already exists");
                }

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
                    StreamUrl = GenerateStreamUrl(request.IpAddress, request.Port, request.Username, request.Password),
                    Status = CameraStatus.Offline,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Cameras.Add(camera);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new camera: {Name} with IP: {IP}", request.Name, request.IpAddress);

                return _mapper.Map<CameraDto>(camera);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating camera");
                throw;
            }
        }

        public async Task<CameraDto?> UpdateCameraAsync(int id, UpdateCameraRequest request)
        {
            try
            {
                var camera = await _context.Cameras.FindAsync(id);
                if (camera == null)
                    return null;

                camera.Name = request.Name;
                camera.Location = request.Location;
                camera.Description = request.Description;
                camera.IsActive = request.IsActive;
                camera.RecordingEnabled = request.RecordingEnabled;
                camera.MotionDetectionEnabled = request.MotionDetectionEnabled;
                camera.FaceRecognitionEnabled = request.FaceRecognitionEnabled;
                camera.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated camera: {Name} (ID: {Id})", camera.Name, id);

                return _mapper.Map<CameraDto>(camera);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating camera with id {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteCameraAsync(int id)
        {
            try
            {
                var camera = await _context.Cameras.FindAsync(id);
                if (camera == null)
                    return false;

                // Soft delete - just mark as inactive
                camera.IsActive = false;
                camera.UpdatedAt = DateTime.UtcNow;

                // Or hard delete if needed
                _context.Cameras.Remove(camera);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted camera: {Name} (ID: {Id})", camera.Name, id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting camera with id {Id}", id);
                throw;
            }
        }

        public async Task<bool> TestCameraConnectionAsync(int id)
        {
            try
            {
                var camera = await _context.Cameras.FindAsync(id);
                if (camera == null)
                    return false;

                // Simple ping test
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(camera.IpAddress, 5000);

                var isOnline = reply.Status == IPStatus.Success;

                // Update camera status
                camera.Status = isOnline ? CameraStatus.Online : CameraStatus.Offline;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Camera {Name} connection test: {Status}",
                    camera.Name, isOnline ? "Success" : "Failed");

                return isOnline;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing camera connection for id {Id}", id);
                return false;
            }
        }

        public async Task<string> GetStreamUrlAsync(int id, string quality = "HD")
        {
            try
            {
                var camera = await _context.Cameras.FindAsync(id);
                if (camera == null)
                    return string.Empty;

                // Generate stream URL based on quality
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

        public async Task<IEnumerable<CameraDto>> GetCamerasByLocationAsync(string location)
        {
            try
            {
                var cameras = await _context.Cameras
                    .Where(c => c.Location.Contains(location) && c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<CameraDto>>(cameras);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cameras by location: {Location}", location);
                throw;
            }
        }

        public async Task<bool> UpdateCameraStatusAsync(int id, CameraStatus status)
        {
            try
            {
                var camera = await _context.Cameras.FindAsync(id);
                if (camera == null)
                    return false;

                camera.Status = status;
                camera.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated camera {Name} status to {Status}", camera.Name, status);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating camera status for id {Id}", id);
                return false;
            }
        }

        public async Task<Dictionary<string, object>> GetCameraStatsAsync(int id)
        {
            try
            {
                var camera = await _context.Cameras
                    .Include(c => c.Events)
                    .Include(c => c.Recordings)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (camera == null)
                    return new Dictionary<string, object>();

                var today = DateTime.UtcNow.Date;
                var thisWeek = DateTime.UtcNow.AddDays(-7);

                return new Dictionary<string, object>
                {
                    { "totalEvents", camera.Events.Count },
                    { "eventsToday", camera.Events.Count(e => e.Timestamp.Date == today) },
                    { "eventsThisWeek", camera.Events.Count(e => e.Timestamp >= thisWeek) },
                    { "totalRecordings", camera.Recordings.Count },
                    { "recordingsToday", camera.Recordings.Count(r => r.StartTime.Date == today) },
                    { "totalStorageGB", Math.Round(camera.Recordings.Sum(r => r.FileSizeBytes) / (1024.0 * 1024 * 1024), 2) },
                    { "status", camera.Status.ToString() },
                    { "uptime", camera.Status == CameraStatus.Online ? "Online" : "Offline" }
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
            // RTSP URL format for IP cameras
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                return $"rtsp://{username}:{password}@{ip}:{port}/stream1";
            }
            return $"rtsp://{ip}:{port}/stream1";
        }
    }
}