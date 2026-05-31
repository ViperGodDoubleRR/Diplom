using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;


namespace CommentService.Infrastructure.Data
{
    public class CommentDbContextFactory
        : IDesignTimeDbContextFactory<CommentDbContext>
    {
        public CommentDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder =
                new DbContextOptionsBuilder<CommentDbContext>();

            optionsBuilder.UseSqlServer(
                "Server=localhost,1433;" +
                "Database=CommentDb;" +
                "User Id=sa;" +
                "Password=YourStrong!Passw0rd;" +
                "TrustServerCertificate=True;");

            return new CommentDbContext(optionsBuilder.Options);
        }
    }
}
