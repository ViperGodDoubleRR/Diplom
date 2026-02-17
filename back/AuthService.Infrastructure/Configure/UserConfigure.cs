using AuthService.Domain.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Configure
{
    public class UserConfigure : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);
            builder
                .HasMany(u => u.VerificationCodes)
                .WithOne(v => v.UserCode)
                .HasForeignKey(v => v.CodeUserId)
                .OnDelete(DeleteBehavior.Cascade);
            builder
                .HasOne(u => u.TableToken)
                .WithOne(tt => tt.UserToken)
                .HasForeignKey<TableToken>(tt => tt.TokenUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
