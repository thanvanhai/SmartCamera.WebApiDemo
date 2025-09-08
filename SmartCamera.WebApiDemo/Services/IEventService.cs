using AutoMapper;

using SmartCamera.WebApiDemo.Data;
using SmartCamera.WebApiDemo.DTOs;
using SmartCamera.WebApiDemo.Models;

namespace SmartCamera.WebApiDemo.Services
{
    public interface IEventService
    {
        Task<IEnumerable<EventDto>> GetEventsAsync(int? cameraId = null, int page = 1, int pageSize = 50);
        Task<EventDto?> GetEventByIdAsync(int id);
        Task<EventDto> CreateEventAsync(Event eventEntity);
        Task<bool> AcknowledgeEventAsync(int id, AcknowledgeEventRequest request);
        Task<IEnumerable<EventDto>> GetUnacknowledgedEventsAsync();
        Task<Dictionary<string, object>> GetEventStatsAsync(int? cameraId = null);
    }
}
