using ChatService.Domain.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatService.Infrastructure.Configure
{
    public class MessageMediaConfigure : IEntityTypeConfiguration<MessageMedia>
    {
        public void Configure(EntityTypeBuilder<MessageMedia> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Bucket).IsRequired();
            builder.Property(x => x.FileKey).IsRequired();
            builder.Property(x => x.OriginalName).IsRequired();
            builder.Property(x => x.MediaType).IsRequired();
            builder.Property(x => x.ContentType).IsRequired();
            builder.Property(x => x.Size).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();

            builder.HasIndex(x => x.MessageId);

            builder.HasOne(x => x.Message)
                .WithMany(x => x.Media)   // 🔥 FIX HERE
                .HasForeignKey(x => x.MessageId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}