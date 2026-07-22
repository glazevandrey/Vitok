using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using web_server.Services.Interfaces;
using web_server.Models.DBModels;
using web_server.Models;

namespace web_server.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IHubContext<NotifHub> _hubContext;

        public AuthController(IAuthService authService, IHubContext<NotifHub> hubContext)
        {
            _authService = authService;
            _hubContext = hubContext;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
        {
            if (model == null || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest(new { error = "Неверные параметры входа" });
            }

            string jsonResult;
            if (string.IsNullOrEmpty(model.ExtraParam))
            {
                jsonResult = await _authService.LogIn(model.Email, model.Password, HttpContext, _hubContext);
            }
            else
            {
                jsonResult = await _authService.LogIn(model.Email, model.Password, model.ExtraParam, HttpContext, _hubContext);
            }

            // Возвращаем Content со статусом 200, так как ваш _authService внутри уже генерирует JSON-строку
            return Content(jsonResult, "application/json");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Student registerModel, [FromQuery] string id)
        {
            if (registerModel == null)
            {
                return BadRequest(new { error = "Данные регистрации пусты" });
            }

            var jsonResult = await _authService.Register(registerModel, id, HttpContext, _hubContext);
            return Content(jsonResult, "application/json");
        }

        [Authorize]
        [HttpGet("user")]
        public async Task<IActionResult> GetUser([FromQuery] string args)
        {
            if (string.IsNullOrEmpty(args)) return BadRequest(new { error = "Токен пуст" });
            var json = await _authService.GetUserByToken(args);
            return Ok(json);
        }

        [Authorize]
        [HttpGet("lite-user")]
        public async Task<IActionResult> GetLiteUser([FromQuery] string args)
        {
            if (string.IsNullOrEmpty(args)) return BadRequest(new { error = "Токен пуст" });
            var json = await _authService.GetLiteUserByToken(args);
            return Content(json, "application/json");
        }

        [Authorize]
        [HttpGet("lite-user/{id}")]
        public async Task<IActionResult> GetLiteUserById(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest(new { error = "ID пуст" });
            var json = await _authService.GetLiteUserById(id);
            return Content(json, "application/json");
        }

        // 1. Проверка текущей авторизации при загрузке React-приложения
        [HttpGet("check")]
        public IActionResult CheckAuth()
        {
            // Проверяем, авторизован ли пользователь через стандартный механизм ASP.NET
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // 1. Вытаскиваем NameIdentifier (ID юзера) и Имя
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var displayName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Пользователь";

                // 2. Вытаскиваем роль (Tutor, Student или Manager)
                var role = User.FindFirst("Role")?.Value;

                // 3. Тут твоя бизнес-логика: если это студент, можно сходить в базу 
                // или сервис и достать количество оставшихся занятий (lessonsCount)
                int lessonsCount = 0;
                bool firstLogin = false;

                if (role == "Student" && !string.IsNullOrEmpty(userId))
                {
                    // Пример: lessonsCount = _database.GetRemainingLessons(userId);
                    // firstLogin = _database.IsFirstLogin(userId);
                    lessonsCount = 5; // Временно захардкодим для теста
                    firstLogin = true;
                }

                // Возвращаем объект в camelCase (System.Text.Json сам переведет первую букву в маленькую)
                return Ok(new
                {
                    isAuthenticated = true,
                    user = new
                    {
                        id = userId,
                        displayName = displayName,
                        role = role, // Должно строго возвращать "Tutor", "Student" или "Manager"
                        photoUrl = "/content/images/avatar.png", // Или путь из базы, если есть
                        lessonsCount = lessonsCount,
                        firstLogin = firstLogin
                    }
                });
            }

            // Если куки нет или она протухла
            return Ok(new { isAuthenticated = false, user = (object)null });
        }
    }

    // DTO для красивого приема JSON данных от React
    public class LoginRequestDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string ExtraParam { get; set; } // Для третьего необязательного аргумента
    }
}