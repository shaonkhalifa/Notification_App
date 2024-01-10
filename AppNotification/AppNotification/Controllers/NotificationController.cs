using AppNotification.Enitiy;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;

namespace AppNotification.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NotificationController : ControllerBase
{
    private readonly TelegramBotClient _botClient;

    public NotificationController(TelegramBotClient botClient)
    {
        _botClient = botClient;
    }

    [HttpPost("send-notification")]
    public async Task<IActionResult> SendNotification([FromBody] NotificationRequest request)
    {
        try
        {
            var b = "6189079292";
            await _botClient.SendTextMessageAsync(request.ChatId, request.Message);
            return Ok("Notification sent successfully");
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Failed to send notification");
        }
    }

    [HttpGet("get-chat-id")]
    public IActionResult GetChatId()
    {
        try
        {

            long chatId = GetChatIdFromRequest();

            if (chatId != 0)
            {
                return Ok($"Your Chat ID: {chatId}");
            }
            else
            {
                return StatusCode(500, "Failed to retrieve Chat ID");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Failed to retrieve Chat ID");
        }
    }

    private long GetChatIdFromRequest()
    {

        if (HttpContext.Request.Query.TryGetValue("chatId", out var chatIdValue))
        {
            if (long.TryParse(chatIdValue, out var chatId))
            {
                return chatId;
            }
        }

        return 0;
    }

}
