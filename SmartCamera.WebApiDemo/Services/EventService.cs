using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartCamera.WebApiDemo.Data;
using SmartCamera.WebApiDemo.DTOs;
using SmartCamera.WebApiDemo.Models;
using SmartCamera.WebApiDemo.Services;

namespace SmartCamera.WebApiDemo.Services
{
    public class EventService : IEventService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<EventService> _logger;

        public EventService(AppDbContext context, IMapper mapper, ILogger<EventService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<EventDto>> GetEventsAsync(int? cameraId = null, int page = 1, int pageSize = 50)
        {
            try
            {
                var query = _context.Events
                    .Include(e => e.Camera)
                    .AsQueryable();

                if (cameraId.HasValue)
                {
                    query = query.Where(e => e.CameraId == cameraId.Value);
                }

                var events = await query
                    .OrderByDescending(e => e.Timestamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<EventDto>>(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting events");
                throw;
            }
        }

        public async Task<EventDto?> GetEventByIdAsync(int id)
        {
            try
            {
                var eventEntity = await _context.Events
                    .Include(e => e.Camera)
                    .FirstOrDefaultAsync(e => e.Id == id);

                return eventEntity != null ? _mapper.Map<EventDto>(eventEntity) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event with id {Id}", id);
                throw;
            }
        }

        public async Task<EventDto> CreateEventAsync(Event eventEntity)
        {
            try
            {
                _context.Events.Add(eventEntity);
                await _context.SaveChangesAsync();

                // Load camera info
                await _context.Entry(eventEntity)
                    .Reference(e => e.Camera)
                    .LoadAsync();

                _logger.LogInformation("Created new event: {Title} for camera {CameraName}",
                    eventEntity.Title, eventEntity.Camera.Name);

                return _mapper.Map<EventDto>(eventEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event");
                throw;
            }
        }

        public async Task<bool> AcknowledgeEventAsync(int id, AcknowledgeEventRequest request)
        {
            try
            {
                var eventEntity = await _context.Events.FindAsync(id);
                if (eventEntity == null)
                    return false;

                eventEntity.IsAcknowledged = true;
                eventEntity.AcknowledgedBy = request.AcknowledgedBy;
                eventEntity.AcknowledgedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Event {Id} acknowledged by {User}", id, request.AcknowledgedBy);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error acknowledging event with id {Id}", id);
                return false;
            }
        }

        public async Task<IEnumerable<EventDto>> GetUnacknowledgedEventsAsync()
        {
            try
            {
                var events = await _context.Events
                    .Include(e => e.Camera)
                    .Where(e => !e.IsAcknowledged)
                    .OrderByDescending(e => e.Timestamp)
                    .Take(100) // Limit to prevent too many results
                    .ToListAsync();

                return _mapper.Map<IEnumerable<EventDto>>(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unacknowledged events");
                throw;
            }
        }

        public async Task<Dictionary<string, object>> GetEventStatsAsync(int? cameraId = null)
        {
            try
            {
                var query = _context.Events.AsQueryable();

                if (cameraId.HasValue)
                {
                    query = query.Where(e => e.CameraId == cameraId.Value);
                }

                var today = DateTime.UtcNow.Date;
                var thisWeek = DateTime.UtcNow.AddDays(-7);
                var thisMonth = DateTime.UtcNow.AddDays(-30);

                var stats = new Dictionary<string, object>
                {
                    { "totalEvents", await query.CountAsync() },
                    { "eventsToday", await query.CountAsync(e => e.Timestamp.Date == today) },
                    { "eventsThisWeek", await query.CountAsync(e => e.Timestamp >= thisWeek) },
                    { "eventsThisMonth", await query.CountAsync(e => e.Timestamp >= thisMonth) },
                    { "unacknowledgedEvents", await query.CountAsync(e => !e.IsAcknowledged) },
                    { "criticalEvents", await query.CountAsync(e => e.Severity == EventSeverity.Critical) },
                    { "eventsByType", await query.GroupBy(e => e.Type)
                        .Select(g => new { Type = g.Key.ToString(), Count = g.Count() })
                        .ToListAsync() },
                    { "eventsBySeverity", await query.GroupBy(e => e.Severity)
                        .Select(g => new { Severity = g.Key.ToString(), Count = g.Count() })
                        .ToListAsync() }
                };

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event stats");
                return new Dictionary<string, object>();
            }
        }
    }
}