using SmartCamera.WebApiDemo.DTOs;
using SmartCamera.WebApiDemo.Models;
using SmartCamera.WebApiDemo.DTOs;
using SmartCamera.WebApiDemo.Models;

namespace SmartCamera.WebApiDemo.Services
{
    public interface ICameraService
    {
        Task<IEnumerable<CameraDto>> GetAllCamerasAsync();
        Task<CameraDto?> GetCameraByIdAsync(int id);
        Task<CameraDto> CreateCameraAsync(CreateCameraRequest request);
        Task<CameraDto?> UpdateCameraAsync(int id, UpdateCameraRequest request);
        Task<bool> DeleteCameraAsync(int id);
        Task<bool> TestCameraConnectionAsync(int id);
        Task<string> GetStreamUrlAsync(int id, string quality = "HD");
        Task<IEnumerable<CameraDto>> GetCamerasByLocationAsync(string location);
        Task<bool> UpdateCameraStatusAsync(int id, CameraStatus status);
        Task<Dictionary<string, object>> GetCameraStatsAsync(int id);
    }
}