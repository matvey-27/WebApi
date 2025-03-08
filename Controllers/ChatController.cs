using Microsoft.AspNetCore.Mvc;

namespace WebAPIApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        [HttpGet("ListChat")]
        public ActionResult<List<Chat>> ListChat(string token)
        {
            try
            {
                // Проверка входных данных
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest(new { Message = "Токен обязателен." });
                }

                // Получаем список чатов
                var chats = DataBase.Data.GetChatsByToken(token);
                return Ok(chats);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}