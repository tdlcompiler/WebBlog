using Microsoft.EntityFrameworkCore;
using WebBlog.Models;

namespace WebBlog.Repository
{
    public class BlogDbContext : DbContext
    {
        public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options) { }

        public DbSet<PostModel> Posts { get; set; }
        public DbSet<UserModel> Users { get; set; }
        public DbSet<ImageModel> Images { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PostModel>()
                .HasMany(p => p.Images)
                .WithOne()
                .HasForeignKey(i => i.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
