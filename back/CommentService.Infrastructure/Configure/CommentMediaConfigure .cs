using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommentService.Domain.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommentService.Infrastructure.Configure
{
    public class CommentMediaConfigure : IEntityTypeConfiguration<CommentMedia>
    {
        public void Configure(EntityTypeBuilder<CommentMedia> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Bucket).IsRequired();
            builder.Property(x => x.FileKey).IsRequired();
            builder.Property(x => x.OriginalName).IsRequired();
            builder.Property(x => x.MediaType).IsRequired();
            builder.Property(x => x.ContentType).IsRequired();
            builder.Property(x => x.Size).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();

            builder.HasIndex(x => x.CommentId).IsUnique();
        }
    }
}

