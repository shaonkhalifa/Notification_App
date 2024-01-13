using System.ComponentModel.DataAnnotations;

namespace AppNotification.Enitiy
{
    public class UserInfo
    {
        [Key]
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public long? TelegramNotificationId { get; set; }
        public long? TelegramUserId { get; set; }
    }
}
