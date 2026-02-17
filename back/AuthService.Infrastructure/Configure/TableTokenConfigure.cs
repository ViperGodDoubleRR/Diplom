using AuthService.Domain.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Configure
{
    public class TableTokenConfigure : IEntityTypeConfiguration<TableToken>
    {
        public void Configure(EntityTypeBuilder<TableToken> builder)
        {
            builder.HasKey(tt => tt.Id);
           
        }
    }
}

