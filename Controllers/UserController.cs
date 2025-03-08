using Microsoft.AspNetCore.Mvc;

namespace WebAPIApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        [HttpPost("authenticate")]
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
                return Ok(new { Message = "found user", Token = DataBase.Data.GetTokenByLogin(login) });
            }
            else
            {
                // Пользователь не найден
                return Unauthorized(new { Message = "not found user" });
            }
        }

        [HttpPost("AddUser")]
        public async Task<ActionResult<object>> AddUser(string login, string password)
        {
            // Проверка входных данных
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                return BadRequest(new { Message = "Логин и пароль обязательны." });
            }

            // Асинхронная проверка пользователя
            bool userExists = await DataBase.Data.UserExists(login);

            if (!userExists)
            {
                DataBase.Data.AddUser(login, password);
                return Ok(new { Message = "add user +", Token = DataBase.Data.GetTokenByLogin(login) });
            }
            else
            {
                // Пользователь не найден
                return Unauthorized(new { Message = "user exists" });
            }
        }
    }
}