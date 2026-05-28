using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PostService.Domain.Models;

namespace PostService.Infrastructure.Configure
{
    public class PostMediaConfigure
       : IEntityTypeConfiguration<PostMedia>
    {
        public void Configure(
            EntityTypeBuilder<PostMedia> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.FileKey)
                .IsRequired();

            builder.Property(x => x.Bucket)
                .IsRequired();

            builder.Property(x => x.MediaType)
                .IsRequired();

            builder.Property(x => x.ContentType)
                .IsRequired();
        }
    }
}
