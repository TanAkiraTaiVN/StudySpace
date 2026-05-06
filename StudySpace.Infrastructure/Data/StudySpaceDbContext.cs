using Microsoft.EntityFrameworkCore;
using StudySpace.Core.Entities;
using StudySpace.Core.Enums;

namespace StudySpace.Infrastructure.Data;

public class StudySpaceDbContext : DbContext
{
    public StudySpaceDbContext(DbContextOptions<StudySpaceDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<StudyGroup> StudyGroups => Set<StudyGroup>();
    public DbSet<GroupMember> GroupMembers => Set<GroupMember>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ProgressEntry> ProgressEntries => Set<ProgressEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.FullName).HasMaxLength(100);
            e.Property(x => x.Email).HasMaxLength(100);
            e.Property(x => x.Phone).HasMaxLength(20);
            e.Property(x => x.AvatarUrl).HasMaxLength(500);
            e.Property(x => x.Bio).HasMaxLength(500);
        });

        modelBuilder.Entity<StudyGroup>(e =>
        {
            e.HasIndex(x => x.InviteCode).IsUnique();
            e.Property(x => x.Name).HasMaxLength(100);
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.Subject).HasMaxLength(100);
            e.Property(x => x.CoverImageUrl).HasMaxLength(500);
            e.Property(x => x.InviteCode).HasMaxLength(20);
            e.HasOne(x => x.CreatedBy)
                .WithMany(u => u.CreatedGroups)
                .HasForeignKey(x => x.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<GroupMember>(e =>
        {
            e.HasIndex(x => new { x.StudyGroupId, x.UserId }).IsUnique();
            e.HasOne(x => x.StudyGroup)
                .WithMany(g => g.Members)
                .HasForeignKey(x => x.StudyGroupId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.User)
                .WithMany(u => u.GroupMemberships)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Document>(e =>
        {
            e.Property(x => x.Title).HasMaxLength(200);
            e.Property(x => x.Description).HasMaxLength(1000);
            e.Property(x => x.FileName).HasMaxLength(255);
            e.Property(x => x.StoredFileName).HasMaxLength(255);
            e.Property(x => x.FileUrl).HasMaxLength(500);
            e.Property(x => x.ContentType).HasMaxLength(100);
            e.Property(x => x.Tags).HasMaxLength(200);
            e.HasOne(x => x.StudyGroup)
                .WithMany(g => g.Documents)
                .HasForeignKey(x => x.StudyGroupId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.UploadedBy)
                .WithMany(u => u.UploadedDocuments)
                .HasForeignKey(x => x.UploadedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Schedule>(e =>
        {
            e.Property(x => x.Title).HasMaxLength(200);
            e.Property(x => x.Description).HasMaxLength(1000);
            e.Property(x => x.Location).HasMaxLength(200);
            e.Property(x => x.MeetingUrl).HasMaxLength(500);
            e.HasOne(x => x.StudyGroup)
                .WithMany(g => g.Schedules)
                .HasForeignKey(x => x.StudyGroupId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChatMessage>(e =>
        {
            e.Property(x => x.Content).HasMaxLength(2000);
            e.Property(x => x.AttachmentUrl).HasMaxLength(500);
            e.HasOne(x => x.StudyGroup)
                .WithMany(g => g.Messages)
                .HasForeignKey(x => x.StudyGroupId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Sender)
                .WithMany(u => u.Messages)
                .HasForeignKey(x => x.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Notification>(e =>
        {
            e.Property(x => x.Title).HasMaxLength(200);
            e.Property(x => x.Message).HasMaxLength(1000);
            e.Property(x => x.Link).HasMaxLength(500);
            e.HasOne(x => x.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProgressEntry>(e =>
        {
            e.Property(x => x.Title).HasMaxLength(200);
            e.Property(x => x.Description).HasMaxLength(1000);
            e.HasOne(x => x.StudyGroup)
                .WithMany(g => g.ProgressEntries)
                .HasForeignKey(x => x.StudyGroupId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.User)
                .WithMany(u => u.ProgressEntries)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
