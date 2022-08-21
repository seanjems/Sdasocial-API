using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using sdakccapi.Models.Entities;

namespace sdakccapi.Infrastructure
{
    public class sdakccapiDbContext : IdentityDbContext<AppUser>
    {
        public sdakccapiDbContext(DbContextOptions<sdakccapiDbContext> options) : base(options)
        {

        }
        public DbSet<Posts> posts { get; set; }
        public DbSet<Follower> followers { get; set; }
        public DbSet<Like> likes { get; set; }
        public DbSet<Comments> comments { get; set; }



        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            /* Configure your own tables/entities inside here */
            builder.Entity<Like>().HasKey(t => new { t.UserId, t.PostId });
            builder.Entity<Follower>().HasKey(t => new { t.UserId, t.FollowingId });
            builder.Entity<Comments>().HasKey(t => new { t.UserId, t.PostId });

        }


    }
}
