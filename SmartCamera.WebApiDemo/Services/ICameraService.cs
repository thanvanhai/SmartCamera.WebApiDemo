using SmartCamera.WebApiDemo.DTOs;
using SmartCamera.WebApiDemo.Models;

namespace SmartCamera.WebApiDemo.Services
{
    public interface ICameraService
    {
        Task<IEnumerable<CameraDto>> GetAllCamerasAsync(CancellationToken cancellationToken = default);
        Task<CameraDto?> GetCameraByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<CameraDto> CreateCameraAsync(CreateCameraRequest request, CancellationToken cancellationToken = default);
        Task<CameraDto?> UpdateCameraAsync(int id, UpdateCameraRequest request, CancellationToken cancellationToken = default);
        Task<bool> DeleteCameraAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> TestCameraConnectionAsync(int id, CancellationToken cancellationToken = default);
        Task<string> GetStreamUrlAsync(int id, string quality = "HD", CancellationToken cancellationToken = default);
        Task<IEnumerable<CameraDto>> GetCamerasByLocationAsync(string location, CancellationToken cancellationToken = default);
        Task<bool> UpdateCameraStatusAsync(int id, CameraStatus status, CancellationToken cancellationToken = default);
        Task<Dictionary<string, object>> GetCameraStatsAsync(int id, CancellationToken cancellationToken = default);
    }
}
