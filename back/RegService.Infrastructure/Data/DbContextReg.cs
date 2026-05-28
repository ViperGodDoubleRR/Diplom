using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;

using RegService.Domain.Models;
using RegService.Infrastructure.Configure;

namespace RegService.Infrastructure.Data
{
    public class DbContextReg : DbContext
    {
        public DbContextReg(DbContextOptions<DbContextReg> option)
            : base(option) { }
        public DbSet<User> Users {get;set;}
        public DbSet<VerificationCode> VerificationCodes { get;set;}
        public DbSet<ConfirmedEmail> ConfirmedEmails {get;set;}
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserConfigure());
            modelBuilder.ApplyConfiguration(new VerificationCodeConfigure());
            modelBuilder.ApplyConfiguration(new ConfirmedEmailConfigure());
            modelBuilder.ApplyConfiguration(new OutboxMessageConfigure());

        }
    }
}
