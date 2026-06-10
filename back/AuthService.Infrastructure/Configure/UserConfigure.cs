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

            builder.HasIndex(u => u.Email).IsUnique();
            builder.HasIndex(u => u.Login).IsUnique();

            builder.Property(u => u.Email)
                .HasMaxLength(254)
                .IsRequired();

            builder.Property(u => u.Login)
                .HasMaxLength(32)
                .IsRequired();
            builder
                .HasMany(u => u.VerificationCodes)
                .WithOne(v => v.UserID)
                .HasForeignKey(v => v.CodeUserId)
                .OnDelete(DeleteBehavior.Cascade);
            builder
              .HasMany(u => u.ResetCodes)
              .WithOne(r => r.UserID)
              .HasForeignKey(r => r.ResCodeUserId)
              .OnDelete(DeleteBehavior.Cascade);
            builder
            .HasMany(u => u.Sessions)
            .WithOne(s => s.User)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
