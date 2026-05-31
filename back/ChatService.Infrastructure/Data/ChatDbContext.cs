using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

using ChatService.Domain.Models;
using ChatService.Infrastructure.Configure;

using Microsoft.EntityFrameworkCore;

namespace ChatService.Infrastructure.Data
{
    public class ChatDbContext : DbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options)
            : base(options) { }

        public DbSet<Chat> Chats { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ChatUser> ChatUsers { get; set; }
        public DbSet<ChatMedia> ChatMedia { get; set; }
        public DbSet<MessageMedia> MessageMedia { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ChatConfigure());
            modelBuilder.ApplyConfiguration(new MessageConfigure());
            modelBuilder.ApplyConfiguration(new ChatUserConfigure());
            modelBuilder.ApplyConfiguration(new ChatMediaConfigure());
            modelBuilder.ApplyConfiguration(new MessageMediaConfigure());
        }
    }
}
