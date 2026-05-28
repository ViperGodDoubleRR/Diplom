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
    public class LikedPostConfigure
       : IEntityTypeConfiguration<LikedPost>
    {
        public void Configure(
            EntityTypeBuilder<LikedPost> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new
            {
                x.PostId,
                x.UserId
            }).IsUnique();
        }
    }
}
