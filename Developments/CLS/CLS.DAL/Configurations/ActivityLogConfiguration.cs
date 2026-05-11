using CLS.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CLS.DAL.Configurations;

/// <summary>
/// Fluent API configuration cho bảng activity_logs.
/// Bảng APPEND-ONLY — không có soft delete, không kế thừa BaseEntityConfiguration.
/// </summary>
public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        // ── Table ────────────────────────────────────────────────────────────
        builder.ToTable("activity_logs");

        // ── Primary Key ──────────────────────────────────────────────────────
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasColumnName("id")
            .UseIdentityAlwaysColumn();

        // ── Columns ──────────────────────────────────────────────────────────
        builder.Property(a => a.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(a => a.ActionType)
            .HasColumnName("action_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.Description)
            .HasColumnName("description")
            .HasColumnType("text");

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        // ── Relationships ────────────────────────────────────────────────────
        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
