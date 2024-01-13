using AppNotification.Context;
using AppNotification.Enitiy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;

namespace AppNotification.Services;

public class NotificationService
{
    private readonly ITelegramBotClient _botClient;
    private readonly NDbContext _context;
    public NotificationService(ITelegramBotClient botClient, NDbContext context = null)
    {
        _botClient = botClient;
        _context = context;
    }

    public async Task SendNotification(int userId, string message)
    {
        long? telegramuserId = await GetTelegramUserID(userId);

        await _botClient.SendTextMessageAsync(telegramuserId, message);

    }

    private async Task<long?> GetTelegramUserID(int userId)
    {
        long? q = await _context.UserInfo.Where(a => a.UserId == userId).Select(a => a.TelegramNotificationId).FirstOrDefaultAsync();
        return q;
    }
}
