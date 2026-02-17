
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegService.Domain.Models;

namespace RegService.Infrastructure.Configure
{
    public class VerificationCodeConfigure : IEntityTypeConfiguration<VerificationCode>
    {
        public void Configure(EntityTypeBuilder<VerificationCode> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }
}
