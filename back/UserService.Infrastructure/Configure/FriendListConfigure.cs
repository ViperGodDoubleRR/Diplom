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
    public class FriendListConfigure : IEntityTypeConfiguration<FriendList>
    {
        public void Configure(EntityTypeBuilder<FriendList> builder)
        {
            builder.HasKey(f => new { f.MyId, f.FriendId });
        }
    }
}
