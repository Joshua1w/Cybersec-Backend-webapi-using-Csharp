using Microsoft.EntityFrameworkCore;
using BlogBackend.Models;

namespace BlogBackend.Data
{
    public class BlogDbContext : DbContext
    {
        public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<PostLike> PostLikes { get; set; }
        public DbSet<Subscriber> Subscribers { get; set; }
        // New: Categories table
        public DbSet<Category> Categories { get; set; }

        // OnModelCreating method to define relationships and constraints
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User and Post - One-to-Many relationship
            modelBuilder.Entity<Post>()
                .HasOne(p => p.User)  // A post has one user
                .WithMany(u => u.Posts) // A user can have many posts
                .HasForeignKey(p => p.UserId)  // Foreign key on Post
                .OnDelete(DeleteBehavior.Cascade); // If the user is deleted, delete all posts

            // User and Comment - One-to-Many relationship
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)  // A comment has one user
                .WithMany(u => u.Comments)  // A user can have many comments
                .HasForeignKey(c => c.UserId) // Foreign key on Comment
                .OnDelete(DeleteBehavior.Cascade); // If the user is deleted, delete all comments

            // Post and Comment - One-to-Many relationship
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)  // A comment belongs to one post
                .WithMany(p => p.Comments)  // A post can have many comments
                .HasForeignKey(c => c.PostId) // Foreign key on Comment
                .OnDelete(DeleteBehavior.Cascade); // If the post is deleted, delete all comments

            // User and PostLike - One-to-Many relationship
            modelBuilder.Entity<PostLike>()
                .HasOne(pl => pl.User)  // A like belongs to one user
                .WithMany(u => u.Likes)  // A user can like many posts
                .HasForeignKey(pl => pl.UserId) // Foreign key on PostLike
                .OnDelete(DeleteBehavior.Cascade); // If the user is deleted, delete all likes

            // Post and PostLike - One-to-Many relationship
            modelBuilder.Entity<PostLike>()
                .HasOne(pl => pl.Post)  // A like belongs to one post
                .WithMany(p => p.Likes)  // A post can have many likes
                .HasForeignKey(pl => pl.PostId) // Foreign key on PostLike
                .OnDelete(DeleteBehavior.Cascade); // If the post is deleted, delete all likes

            // Configure Subscriber entity
            modelBuilder.Entity<Subscriber>()
                .HasIndex(s => s.Email)
                .IsUnique();

            // Many-to-many: Post <-> Category
            modelBuilder.Entity<Post>()
                .HasMany(p => p.Categories)
                .WithMany(c => c.Posts)
                .UsingEntity(j => j.ToTable("PostCategories"));

            // Seed the four categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Network Security" },
                new Category { Id = 2, Name = "Application Security" },
                new Category { Id = 3, Name = "Information Security" },
                new Category { Id = 4, Name = "Cloud Security" }
            );
        }
    }
}
