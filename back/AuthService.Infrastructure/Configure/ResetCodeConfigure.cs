using AuthService.Domain.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Configure
{
    public class ResetCodeConfigure : IEntityTypeConfiguration<ResetCode>
    {
        public void Configure(EntityTypeBuilder<ResetCode> builder)
        {
            builder.HasKey(r => r.Id);
        }
    }
}
