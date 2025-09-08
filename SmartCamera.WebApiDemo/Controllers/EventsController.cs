using Microsoft.AspNetCore.Mvc;
using SmartCamera.WebApiDemo.DTOs;
using SmartCamera.WebApiDemo.Services;

namespace SmartCamera.WebApiDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;
        private readonly ILogger<EventsController> _logger;

        public EventsController(IEventService eventService, ILogger<EventsController> logger)
        {
            _eventService = eventService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách events với phân trang
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetEvents(
            [FromQuery] int? cameraId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var events = await _eventService.GetEventsAsync(cameraId, page, pageSize);
                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting events");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Lấy thông tin event theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<EventDto>> GetEvent(int id)
        {
            try
            {
                var eventDto = await _eventService.GetEventByIdAsync(id);
                if (eventDto == null)
                {
                    return NotFound($"Event with ID {id} not found");
                }

                return Ok(eventDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Xác nhận đã xem event
        /// </summary>
        [HttpPost("{id}/acknowledge")]
        public async Task<IActionResult> AcknowledgeEvent(int id, [FromBody] AcknowledgeEventRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _eventService.AcknowledgeEventAsync(id, request);
                if (!result)
                {
                    return NotFound($"Event with ID {id} not found");
                }

                return Ok(new { message = "Event acknowledged successfully", acknowledgedAt = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error acknowledging event {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Lấy danh sách events chưa xác nhận
        /// </summary>
        [HttpGet("unacknowledged")]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetUnacknowledgedEvents()
        {
            try
            {
                var events = await _eventService.GetUnacknowledgedEventsAsync();
                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unacknowledged events");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Lấy thống kê events
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<Dictionary<string, object>>> GetEventStats([FromQuery] int? cameraId = null)
        {
            try
            {
                var stats = await _eventService.GetEventStatsAsync(cameraId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event stats");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}