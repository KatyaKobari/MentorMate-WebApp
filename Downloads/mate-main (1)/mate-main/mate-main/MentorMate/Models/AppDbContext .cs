using Microsoft.EntityFrameworkCore;

namespace MentorMate.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets 
        public DbSet<User> Users { get; set; }
        public DbSet<MentorProfile> MentorProfiles { get; set; }
        public DbSet<MenteeProfile> MenteeProfiles { get; set; }
        public DbSet<MentorshipRequest> MentorshipRequests { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MentorSpacePost> MentorSpacePosts { get; set; }
        public DbSet<MentorSpaceReply> MentorSpaceReplies { get; set; }
        public DbSet<MentorReview> MentorReviews { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureUserRelationships(modelBuilder);
            ConfigureMentorshipRelationships(modelBuilder);
            ConfigureChatRelationships(modelBuilder);
            ConfigureMentorSpaceRelationships(modelBuilder);
            ConfigureReviewRelationships(modelBuilder);
            ConfigureNotificationRelationships(modelBuilder);
        }

        private void ConfigureUserRelationships(ModelBuilder modelBuilder)
        {
            // User -> MentorProfile (1:0..1)
            modelBuilder.Entity<User>()
                .HasOne(u => u.MentorProfile)
                .WithOne(mp => mp.User)
                .HasForeignKey<MentorProfile>(mp => mp.MentorId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> MenteeProfile (1:0..1)
            modelBuilder.Entity<User>()
                .HasOne(u => u.MenteeProfile)
                .WithOne(mp => mp.User)
                .HasForeignKey<MenteeProfile>(mp => mp.MenteeId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private void ConfigureMentorshipRelationships(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MentorshipRequest>()
                .HasOne(m => m.Mentee)
                .WithMany(u => u.MenteeRequests)
                .HasForeignKey(m => m.MenteeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MentorshipRequest>()
                .HasOne(m => m.Mentor)
                .WithMany(u => u.MentorRequests)
                .HasForeignKey(m => m.MentorId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private void ConfigureChatRelationships(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Chat>()
                .HasOne(c => c.Sender)
                .WithMany(u => u.Chats)
                .HasForeignKey(c => c.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Chat>()
                .HasOne(c => c.MentorshipRequest)
                .WithMany(m => m.Chats)
                .HasForeignKey(c => c.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

            // إعداد علاقات الرسائل
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.Messages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private void ConfigureMentorSpaceRelationships(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MentorSpacePost>()
                .HasOne(p => p.CreatedBy)
                .WithMany(u => u.MentorPosts)
                .HasForeignKey(p => p.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MentorSpaceReply>()
                .HasOne(r => r.CreatedBy)
                .WithMany(u => u.MentorReplies)
                .HasForeignKey(r => r.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MentorSpaceReply>()
                .HasOne(r => r.Post)
                .WithMany(p => p.Replies)
                .HasForeignKey(r => r.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private void ConfigureReviewRelationships(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MentorReview>()
                .HasOne(r => r.Mentor)
                .WithMany()
                .HasForeignKey(r => r.MentorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MentorReview>()
                .HasOne(r => r.Mentee)
                .WithMany()
                .HasForeignKey(r => r.MenteeId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private void ConfigureNotificationRelationships(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}