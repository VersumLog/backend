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
        public DbSet<Savings> Savings { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<PostReaction> PostReactions { get; set; } = null!;

        public DbSet<Like> Likes { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
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
                    .WithMany(u => u.Followers)
                    .HasForeignKey(f => f.FollowingId);
            });

            modelBuilder.Entity<PostReaction>(entity =>
            {
                entity.HasKey(pr => pr.Id);

                // ЗМІНЕНО: тепер один юзер може мати лише один рядок взаємодії з одним конкретним постом
                entity.HasIndex(pr => new { pr.UserId, pr.PostId }).IsUnique();

                entity.HasOne(pr => pr.User)
                    .WithMany()
                    .HasForeignKey(pr => pr.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pr => pr.Post)
                    .WithMany()
                    .HasForeignKey(pr => pr.PostId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Dictionary>()
           .HasIndex(d => new { d.UserId, d.PostId, d.AnchorId })
           .IsUnique();

           modelBuilder.Entity<Savings>()
         .HasKey(s => new { s.UserId, s.PostId });

            modelBuilder.Entity<Savings>()
        .HasIndex(s => new { s.UserId, s.PostId });
            modelBuilder.Entity<Like>()
            .HasIndex(l => new { l.UserId, l.PostId })
            .IsUnique(); 

            modelBuilder.Entity<Like>()
                .HasOne(l => l.Post)
                .WithMany(p => p.Likes)
                .HasForeignKey(l => l.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Like>()
                .HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
