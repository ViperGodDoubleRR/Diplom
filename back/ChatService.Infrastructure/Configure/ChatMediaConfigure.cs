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
    public class ChatMediaConfigure : IEntityTypeConfiguration<ChatMedia>
    {
        public void Configure(EntityTypeBuilder<ChatMedia> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Bucket)
                .IsRequired();

            builder.Property(x => x.FileKey)
                .IsRequired();

            builder.Property(x => x.OriginalName)
                .IsRequired();

            builder.Property(x => x.MediaType)
                .IsRequired();

            builder.Property(x => x.ContentType)
                .IsRequired();

            builder.Property(x => x.Size)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasIndex(x => x.ChatId);

            builder.HasOne(x => x.Chat)
                .WithMany(x => x.Media)
                .HasForeignKey(x => x.ChatId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
