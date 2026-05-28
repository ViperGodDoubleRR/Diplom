using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using UserService.Domain.Models;
using UserService.Infrastructure.Configure;

namespace UserService.Infrastructure.Data
{
    public class DbContextUser : DbContext
    {
        public DbContextUser(DbContextOptions<DbContextUser> option)
            : base(option) { }
        public DbSet<User> Users { get; set; }
        public DbSet<FriendList> FriendLists { get; set; }
        public DbSet<BlackList> BlackLists { get; set; }
        public DbSet<MediaUser> MediaUsers { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserConfigure());
            modelBuilder.ApplyConfiguration(new FriendListConfigure());
            modelBuilder.ApplyConfiguration(new BlackListConfigure());
            modelBuilder.ApplyConfiguration(new MediaUserConfigure());
        }
    }
}
