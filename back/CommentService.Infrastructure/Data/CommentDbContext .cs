using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using CommentService.Domain.Models;
using CommentService.Infrastructure.Configure;

using Microsoft.EntityFrameworkCore;



namespace CommentService.Infrastructure.Data
{
    public class CommentDbContext : DbContext
    {
        public CommentDbContext(DbContextOptions<CommentDbContext> options)
            : base(options) { }

        public DbSet<Comment> Comments { get; set; }
        public DbSet<CommentMedia> CommentMedia { get; set; }
        public DbSet<CommentReaction> CommentReactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new CommentConfigure());
            modelBuilder.ApplyConfiguration(new CommentMediaConfigure());
            modelBuilder.ApplyConfiguration(new CommentReactionConfigure());

        }
    }
}

