using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SafeHabour.Application.Interfaces;
using SafeHabour.Models.Requests;
using SafeHabour.Data.DTOMapper.Schedule;

namespace SafeHabour.API.Controllers
{
    [ApiController]
    [Route("api/serviceworkers/{workerId:guid}/schedules")]
    public class SchedulesController : ControllerBase
    {
        private readonly IScheduleService _service;
        public SchedulesController(IScheduleService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAllForWorker([FromRoute] Guid workerId)
        {
            var entities = await _service.GetWorkerSchedulesAsync(workerId);
            var res = entities.ConvertAll(ScheduleMapper.ToResponse);
            return Ok(res);
        }

        [HttpGet("day")]
        public async Task<IActionResult> GetForDay([FromRoute] Guid workerId, [FromQuery] DayOfWeek dayOfWeek)
        {
            var entities = await _service.GetWorkerSchedulesForDayAsync(workerId, dayOfWeek);
            var res = entities.ConvertAll(ScheduleMapper.ToResponse);
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromRoute] Guid workerId, [FromBody] ScheduleRequest req)
        {
            if (req.ServiceWorkerId == Guid.Empty) req.ServiceWorkerId = workerId;
            if (req.ServiceWorkerId != workerId) return BadRequest("WorkerId mismatch");

            var entity = ScheduleMapper.ToEntity(req);
            var created = await _service.CreateScheduleAsync(entity);
            var res = ScheduleMapper.ToResponse(created);

            return CreatedAtAction(nameof(GetAllForWorker), new { workerId = res.ServiceWorkerId }, res);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid workerId, [FromRoute] Guid id, [FromBody] ScheduleRequest req)
        {
            if (req.ServiceWorkerId == Guid.Empty) req.ServiceWorkerId = workerId;
            if (req.ServiceWorkerId != workerId) return BadRequest("WorkerId mismatch");

            var entity = await _service.GetByIdAsync(id);
            if (entity == null) return NotFound();

            ScheduleMapper.UpdateEntityFromRequest(entity, req);

            var updated = await _service.UpdateScheduleAsync(entity);
            var res = ScheduleMapper.ToResponse(updated);

            return Ok(res);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteScheduleAsync(id);
            return NoContent();
        }
    }
}
