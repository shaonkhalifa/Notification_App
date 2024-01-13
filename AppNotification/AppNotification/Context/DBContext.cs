using AppNotification.Enitiy;
using Microsoft.EntityFrameworkCore;

namespace AppNotification.Context
{
    public class NDbContext : DbContext
    {
        public NDbContext(DbContextOptions<NDbContext> options) : base(options)
        {

        }
        public DbSet<UserInfo> UserInfo { get; set; }
    }
}
