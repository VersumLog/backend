using Microsoft.EntityFrameworkCore;
using Versum.Models;

namespace Versum.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserProfile> Profiles { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Follow> Follows { get; set; } = null!;
        public DbSet<Dictionary> Dictionary { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)

        {
            modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

            modelBuilder.Entity<User>()
            .HasIndex(u => new { u.Email, u.PasswordResetToken });
            modelBuilder.Entity<User>().HasIndex(u => u.EmailConfirmationTokenHash).IsUnique();

            modelBuilder.Entity<User>()
            .HasOne(u => u.Profile)
            .WithOne(p => p.User)
            .HasForeignKey<UserProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Post>()
            .HasOne(p => p.Author)
            .WithMany(a => a.Posts)
            .HasForeignKey(p => p.AuthorId);

            modelBuilder.Entity<Genre>()
            .HasMany(p => p.Posts)
            .WithMany(g => g.Genres);

            modelBuilder.Entity<Follow>(entity =>
            {

                entity.HasKey(f => f.Id);


                entity.HasOne(f => f.Follower)
                    .WithMany()
                    .HasForeignKey(f => f.FollowerId);

                entity.HasOne(f => f.Following)
                    .WithMany()
                    .HasForeignKey(f => f.FollowingId);
            });

            modelBuilder.Entity<Dictionary>()
           .HasIndex(d => new { d.UserId, d.PostId, d.AnchorId })
           .IsUnique();
        }
    }
}
