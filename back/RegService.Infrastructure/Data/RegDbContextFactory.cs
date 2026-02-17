
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RegService.Infrastructure.Data
{
    public class RegDbContextFactory : IDesignTimeDbContextFactory<DbContextReg>
    {
        public DbContextReg CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DbContextReg>();

            optionsBuilder.UseSqlServer(
                "Server=localhost,1433;Database=RegDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;");

            return new DbContextReg(optionsBuilder.Options);
        }
    }

}
