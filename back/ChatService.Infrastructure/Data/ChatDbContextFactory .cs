using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ChatService.Infrastructure.Data
{
    public class ChatDbContextFactory : IDesignTimeDbContextFactory<ChatDbContext>
    {
        public ChatDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ChatDbContext>();

            optionsBuilder.UseSqlServer(
                "Server=localhost,1433;Database=ChatDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;");

            return new ChatDbContext(optionsBuilder.Options);
        }
    }
}
