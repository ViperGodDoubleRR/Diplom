    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    using RegService.Domain.Models;
    namespace RegService.Infrastructure.Configure
    {
        public class ConfirmedEmailConfigure : IEntityTypeConfiguration<ConfirmedEmail>
        {
            public void Configure(EntityTypeBuilder<ConfirmedEmail> builder)
            {
                builder.HasKey(x => x.Id);
            }
        }
    }
