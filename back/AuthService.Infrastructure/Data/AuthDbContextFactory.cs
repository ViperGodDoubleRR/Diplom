using AuthService.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AuthService.Infrastructure.Data
{
    public class AuthDbContextFactory : IDesignTimeDbContextFactory<DbContextAuth>
    {
        public DbContextAuth CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DbContextAuth>();

            optionsBuilder.UseSqlServer(
                "Server=localhost,1433;Database=AuthDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;");

            return new DbContextAuth(optionsBuilder.Options);
        }
    }

}
