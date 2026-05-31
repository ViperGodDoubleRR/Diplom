using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ChatService.Domain.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatService.Infrastructure.Configure
{
    public class ChatUserConfigure : IEntityTypeConfiguration<ChatUser>
    {
        public void Configure(EntityTypeBuilder<ChatUser> builder)
        {
            builder.HasKey(x => new { x.ChatId, x.UserId });

            builder.Property(x => x.Role)
                .IsRequired();

            builder.Property(x => x.JoinedAt)
                .IsRequired();

            builder.HasIndex(x => x.UserId);

            builder.HasOne(x => x.Chat)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.ChatId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
