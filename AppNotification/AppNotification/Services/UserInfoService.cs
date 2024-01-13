using AppNotification.Context;
using Microsoft.EntityFrameworkCore;

namespace AppNotification.Services;

public class UserInfoService
{
    private readonly NDbContext _context;

    public UserInfoService(NDbContext context)
    {
        _context = context;
    }

    public async Task<bool> UserInfoUpdateAsync(string phone, long chatId, long? userId)
    {
      
        var data = await _context.UserInfo.Where(a => a.PhoneNumber == phone).FirstOrDefaultAsync();

        if (data != null)
        {
            data.TelegramUserId = userId ;
            data.TelegramNotificationId = chatId;
            _context.UserInfo.Update(data);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;

    }

    public async Task<bool> FindUserCom(string phone, long chatId, long? userId)
    {
       
        var data = await _context.UserInfo.Where(a => a.PhoneNumber == phone).FirstOrDefaultAsync();
        if (data != null)
        {
            data.TelegramUserId = userId;
            data.TelegramNotificationId = chatId;
            _context.UserInfo.Update(data);
            _context.SaveChanges();
            return true;
        }
        return false;

    }
}
