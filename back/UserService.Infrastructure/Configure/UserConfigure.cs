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
    public class UserConfigure : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);

            builder
                  .HasMany(u => u.MyFriends)
                  .WithOne(f => f.My)
                  .HasForeignKey(f => f.MyId)
                  .OnDelete(DeleteBehavior.Restrict);

            builder
                  .HasMany(u => u.AddedToFriends)
                  .WithOne(f => f.Friend)
                  .HasForeignKey(f => f.FriendId)
                  .OnDelete(DeleteBehavior.Restrict);

            builder
                 .HasMany(u => u.MyBlackList)
                 .WithOne(b => b.My)
                 .HasForeignKey(b => b.MyId)
                 .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasMany(u => u.BlockedByUsers)
                .WithOne(b => b.Black)
                .HasForeignKey(b => b.BlackId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasMany(u => u.MediaUsers)
                .WithOne(m => m.User)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(u => u.Email)
                   .IsUnique();

            builder.HasIndex(u => u.Login)
                   .IsUnique();
        }
    }
}
