using Microsoft.AspNetCore.Mvc;

namespace WebAPIApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessegesController : ControllerBase
    {
        [HttpPost("SendText")]
        public async Task<ActionResult<object>> SendText(string token, string companion_saqura_id, string text)
        {
            // Проверка входных данных
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(companion_saqura_id))
            {
                return BadRequest(new { Message = "fail param" });
            }

             // Получаем saqura_id по токену
            string? saquraId = DataBase.Data.GetIdByToken(token);
            if (saquraId == null)
            {
                return BadRequest(new { Message = "fail token" });
            } 

            int one_id = Int32.Parse(DataBase.Data.GetIdByToken(token));
            int companion_id = Int32.Parse(companion_saqura_id);

            // Асинхронная проверка chat
            bool chatExists = await DataBase.Data.ChatExistsAsync(one_id, companion_id);

            if (chatExists)
            {
                DataBase.Data.AddMessage(DataBase.Data.FindChat(one_id, companion_id),
                                        one_id,
                                        text,
                                        "1");
                return Ok(new { Message = "messege send" });

            }
            else
            {
                DataBase.Data.CreateChat(one_id, companion_id);
                // Пользователь не найден
                DataBase.Data.AddMessage(DataBase.Data.FindChat(one_id, companion_id),
                                        one_id,
                                        text,
                                        "1");
                return Ok(new { Message = "messege send" });
            }
        }
    }
}