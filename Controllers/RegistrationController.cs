using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace WebAPIApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        [HttpGet("authenticate")]
        public async Task<ActionResult<object>> Authenticate(string login, string password)
        {
            // Проверка входных данных
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                return BadRequest(new { Message = "Логин и пароль обязательны." });
            }

            // Асинхронная проверка пользователя
            bool userExists = await DataBase.Data.FindUserAsync(login, password);

            if (userExists)
            {
                return Ok(new { Message = "верный логин и пароль" });
            }
            else
            {
                // Пользователь не найден
                return Unauthorized(new { Message = "Неверный логин или пароль" });
            }
        }
    }
}