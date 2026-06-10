using System;
using System.Collections.Generic;
using System.Text;

using AuthService.Domain.Models;
using AuthService.Infrastructure.Configure;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Data
{
    public class DbContextAuth : DbContext
    {
        public DbContextAuth(DbContextOptions<DbContextAuth> option)
            : base(option) { }
        public DbSet<User> Users {get;set;}
        public DbSet<VerificationCode> VerificationCodes { get;set;}
        public DbSet<UserSession> UserSessions {get;set;}
        public DbSet<ResetCode> ResetCodes { get;set;}
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserConfigure());
            modelBuilder.ApplyConfiguration(new VerificationCodeConfigure());
            modelBuilder.ApplyConfiguration(new UserSessionTableConfigure());
            modelBuilder.ApplyConfiguration(new ResetCodeConfigure());
            modelBuilder.Ignore<UserEmailHistory>();
        }
    }
}
