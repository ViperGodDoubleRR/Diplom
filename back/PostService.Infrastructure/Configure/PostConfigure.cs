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
    public class PostConfigure
       : IEntityTypeConfiguration<Post>
    {
        public void Configure(
            EntityTypeBuilder<Post> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Description)
                .HasMaxLength(3000);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasMany(x => x.Media)
                .WithOne(x => x.Post)
                .HasForeignKey(x => x.PostId);

            builder.HasMany(x => x.Reactions)
                .WithOne(x => x.Post)
                .HasForeignKey(x => x.PostId);

            builder.HasMany(x => x.Likes)
                .WithOne(x => x.Post)
                .HasForeignKey(x => x.PostId);

            builder.HasMany(x => x.Favorites)
                .WithOne(x => x.Post)
                .HasForeignKey(x => x.PostId);
        }
    }
}
