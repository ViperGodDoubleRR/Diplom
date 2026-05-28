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
    public class BlackListConfigure : IEntityTypeConfiguration<BlackList>
    {
        public void Configure(EntityTypeBuilder<BlackList> builder)
        {
            builder.HasKey(b => new { b.MyId, b.BlackId });
        }
    }
}
