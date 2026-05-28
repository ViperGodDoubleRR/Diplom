using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UserService.Infrastructure.Data
{
    public class UserDbContextFactory : IDesignTimeDbContextFactory<DbContextUser>
    {
        public DbContextUser CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DbContextUser>();

            optionsBuilder.UseSqlServer(
                "Server=localhost,1433;Database=UserDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;");

            return new DbContextUser(optionsBuilder.Options);
        }
    }
}
