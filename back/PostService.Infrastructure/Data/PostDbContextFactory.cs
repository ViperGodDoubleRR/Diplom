using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PostService.Infrastructure.Data
{
    public class PostDbContextFactory
         : IDesignTimeDbContextFactory<DbContextPost>
    {
        public DbContextPost CreateDbContext(
            string[] args)
        {
            var optionsBuilder =
                new DbContextOptionsBuilder<DbContextPost>();

            optionsBuilder.UseSqlServer(
                "Server=localhost,1433;" +
                "Database=PostDb;" +
                "User Id=sa;" +
                "Password=YourStrong!Passw0rd;" +
                "TrustServerCertificate=True;");

            return new DbContextPost(
                optionsBuilder.Options);
        }
    }
}
