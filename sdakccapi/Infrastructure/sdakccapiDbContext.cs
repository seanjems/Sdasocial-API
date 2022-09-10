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
        public DbSet<Chats> chats { get; set; }
        public DbSet<Conversations> conversations { get; set; }
        public DbSet<ConversationMembers> conversationMembers { get; set; }
        public DbSet<ActiveUsers> activeUsers { get; set; }





        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            /* Configure your own tables/entities inside here */
            builder.Entity<Like>().HasKey(t => new { t.UserId, t.PostId });
            builder.Entity<Follower>().HasKey(t => new { t.UserId, t.FollowingId });
            builder.Entity<ConversationMembers>().HasKey(t => new { t.UserId, t.ConversationId });
            builder.Entity<ActiveUsers>().HasKey(t => new { t.ConnectionId });
            
            builder.Entity<ConversationMembers>().HasOne<Conversations>()
                .WithMany(o => o.Members)
                .HasForeignKey(ol => ol.ConversationId);
        }


    }
}
