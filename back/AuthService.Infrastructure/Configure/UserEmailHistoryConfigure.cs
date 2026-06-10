using AuthService.Domain.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Configure
{
    public class UserEmailHistoryConfigure : IEntityTypeConfiguration<UserEmailHistory>
    {
        public void Configure(EntityTypeBuilder<UserEmailHistory> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Email)
                .HasMaxLength(320)
                .IsRequired();

            builder.HasIndex(x => new { x.UserId, x.Email });
        }
    }
}
