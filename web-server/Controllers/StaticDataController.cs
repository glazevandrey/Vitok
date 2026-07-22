using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using web_server.DbContext;
using web_server.Database.Repositories;
using web_server.Services.Interfaces;
using web_server.Database;
using web_server.Models;

namespace web_server.Controllers
{
    [ApiController]
    [Route("api/static")]
    public class StaticDataController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly CourseRepository _courseRepository;
        private readonly ContactsRepository _contactsRepository;
        private readonly UserRepository _userRepository;
        private readonly IStatisticsService _statisticsService;

        public StaticDataController(DataContext context, CourseRepository courseRepository, ContactsRepository contactsRepository, UserRepository userRepository, IStatisticsService statisticsService)
        {
            _context = context;
            _courseRepository = courseRepository;
            _contactsRepository = contactsRepository;
            _userRepository = userRepository;
            _statisticsService = statisticsService;
        }

        [HttpGet("tariffs")]
        public async Task<IActionResult> GetTariffs()
        {
            var tariffs = await _courseRepository.GetAllTariffs();
            return Ok(tariffs);
        }

        [HttpGet("goals")]
        public async Task<IActionResult> GetGoals()
        {
            var goals = await _courseRepository.GetAllGoals();
            return Ok(goals);
        }

        [HttpGet("contacts")]
        public async Task<IActionResult> GetContacts()
        {
            var contacts = await _contactsRepository.GetContacts();
            return Ok(contacts);
        }

        [HttpGet("site-contacts")]
        public async Task<IActionResult> GetSiteContacts()
        {
            var siteContacts = await _context.SiteContacts.ToListAsync();
            return Ok(siteContacts);
        }

        [HttpGet("all-users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var allUsers = await _userRepository.GetAll();
            return Ok(allUsers);
        }

        [Authorize]
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics([FromQuery] string args)
        {
            if (string.IsNullOrEmpty(args)) return BadRequest(new { error = "Параметры пусты" });
            var data = await _statisticsService.FormingStatData(args);
            if (data == null) return BadRequest(new { error = "Ошибка формирования статистики" });
            return Ok(data);
        }
    }
}