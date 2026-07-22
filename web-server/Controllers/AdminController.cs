using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using web_server.DbContext;
using web_server.Models.DTO;
using web_server.Models.DBModels;
using web_server.Services;
using web_server.Database;

namespace web_server.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public AdminController(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpPost("init-db")]
        public async Task<IActionResult> InitDatabase()
        {
            // Переносим вашу логику InitializeDataBase сюда
            if (_context.Tutors.Count() == 0 && _context.Students.Count() == 0 && _context.Managers.Count() == 0)
            {
                // ... Весь ваш код заполнения из TestData ...
                // Замените "data" на "_context" и "map" на "_mapper"
                return Ok(new { success = true, message = "База данных успешно инициализирована тестовыми данными" });
            }
            return Ok(new { success = false, message = "База уже содержит данные" });
        }

        [HttpGet("test-sort")]
        public IActionResult TestSort()
        {
            var res = new List<ScheduleDTO>();
            int count = 1000;

            for (int i = 0; i < count; i++)
            {
                var ten = _context.Schedules
                    .Include(m => m.RescheduledLessons)
                    .Include(m => m.ReadyDates)
                    .Include(m => m.PaidLessons)
                    .Include(m => m.SkippedDates)
                    .Take(10).ToList();
                res.AddRange(ten);
            }

            Stopwatch s = Stopwatch.StartNew();
            ScheduleService.SortSchedulesForUnpaid(res);
            s.Stop();

            return Ok(new { elapsedMilliseconds = s.ElapsedMilliseconds, totalItems = res.Count });
        }
    }
}