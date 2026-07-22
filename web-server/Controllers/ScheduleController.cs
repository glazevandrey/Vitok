using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using web_server.Services.Interfaces;
using web_server.Models;
using web_server.Models.V2;
using web_server.Services;


namespace web_server.Controllers
{
    [ApiController]
    [Route("api/schedule")]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;
        private readonly IHubContext<NotifHub> _hubContext;
        private readonly ILessonsService _lessonsService;
        private readonly ITutorService _tutorService;

        public ScheduleController(ILessonsService lessonsService, IScheduleService scheduleService, IHubContext<NotifHub> hubContext, ITutorService tutorService)
        {
            _lessonsService = lessonsService;
            _scheduleService = scheduleService;
            _hubContext = hubContext;
            _tutorService = tutorService;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<ScheduleWeekResponseDto>> GetSchedules(
          [FromQuery] DateTime startDate,
          [FromQuery] DateTime endDate)
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var role = User.FindFirst("Role").Value;

            var result = await _scheduleService.GetScheduleAsync(userId, role, startDate, endDate);

            return Ok(result);
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> RemoveSchedule(RemoveLessonRequest request)
        {
            await _tutorService.RemoveTutorScheduleV2(request, _hubContext);

            return Ok();

        }

        [HttpPost("lesson/reschedule")]
        [Authorize]
        public async Task<IActionResult> RescheduleLesson(RescheduleLessonDto dto)
        {
            try
            {
                var rescheduled = await _lessonsService.RescheduleLesson(dto, _hubContext);

                if (rescheduled == null)
                {
                    return BadRequest(new { success = false, message = "Выбранная дата занята или некорректна." });
                }

                return Ok(new { success = true, result = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Ошибка при переносе занятия: {ex.Message}" });
            }

        }

      
    }
}