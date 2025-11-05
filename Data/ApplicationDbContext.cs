using Microsoft.EntityFrameworkCore;
using MyApp.Models;

namespace MyApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet 屬性定義資料表
        public DbSet<ChatSession> ChatSessions { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置 ChatSession 實體
            modelBuilder.Entity<ChatSession>(entity =>
            {
                entity.HasKey(e => e.SessionId);
                entity.Property(e => e.SessionId)
                    .HasMaxLength(50)
                    .IsRequired();
                entity.Property(e => e.StartTime)
                    .HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.LastActivity)
                    .HasDefaultValueSql("GETDATE()");
            });

            // 配置 ChatMessage 實體
            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content)
                    .IsRequired();
                entity.Property(e => e.Sender)
                    .HasMaxLength(50)
                    .IsRequired();
                entity.Property(e => e.Timestamp)
                    .HasDefaultValueSql("GETDATE()");

                // 設定與 ChatSession 的關係
                entity.HasOne<ChatSession>()
                    .WithMany(s => s.Messages)
                    .HasForeignKey(e => e.SessionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}