using AppNotification.Context;
using AppNotification.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace AppNotification.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserInfoController : ControllerBase
    {
        private readonly NotificationService _notificationService;
        private readonly NDbContext _context;

        public UserInfoController(NotificationService notificationService, NDbContext context = null)
        {
            _notificationService = notificationService;
            _context = context;
        }

        [HttpGet("userInfo/{userId}")]
        public async Task<IActionResult> GetUserById(int userId)
        {
            try
            {
                var data = await _context.UserInfo.Where(a => a.UserId == userId).FirstOrDefaultAsync();
                var message = "You retrieve user information";

                await _notificationService.SendNotification(userId, message);

                return Ok(data);


            }
            catch (Exception ex)
            {
                return StatusCode(500, "Failed to retrieve Chat ID");
            }
        }
    }
}
