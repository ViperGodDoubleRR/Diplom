
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PostService.Domain;
using PostService.Domain.Models;
namespace PostService.Infrastructure.Configure
{
    public class PostReactionConfigure
        : IEntityTypeConfiguration<PostReaction>
    {
        public void Configure(
            EntityTypeBuilder<PostReaction> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Type)
                .IsRequired();

            // один юзер = одна реакция
            builder.HasIndex(x => new
            {
                x.PostId,
                x.UserId
            }).IsUnique();
        }
    }
}
