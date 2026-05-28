using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using UserService.Domain.Models;
namespace UserService.Infrastructure.Configure
{
    public class MediaUserConfigure:IEntityTypeConfiguration<MediaUser>
    {
        public void Configure(EntityTypeBuilder<MediaUser> builder)
        {
            builder.HasKey(m => m.Id);
        }
    }
}
