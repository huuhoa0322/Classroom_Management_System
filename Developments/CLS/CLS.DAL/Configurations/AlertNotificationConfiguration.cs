using CLS.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CLS.DAL.Configurations;

/// <summary>
/// Fluent API configuration cho bảng alert_notifications.
/// Bảng APPEND-ONLY — không có soft delete, không kế thừa BaseEntityConfiguration.
/// </summary>
public class AlertNotificationConfiguration : IEntityTypeConfiguration<AlertNotification>
{
    public void Configure(EntityTypeBuilder<AlertNotification> builder)
    {
        // ── Table ────────────────────────────────────────────────────────────
        builder.ToTable("alert_notifications");

        // ── Primary Key ──────────────────────────────────────────────────────
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasColumnName("id")
            .UseIdentityAlwaysColumn();

        // ── Columns ──────────────────────────────────────────────────────────
        builder.Property(a => a.StudentPackageId)
            .HasColumnName("student_package_id");

        builder.Property(a => a.TargetEmail)
            .HasColumnName("target_email")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(a => a.Type)
            .HasColumnName("type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.Message)
            .HasColumnName("message")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(a => a.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue("pending");

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(a => a.ConsultedAt)
            .HasColumnName("consulted_at");

        builder.Property(a => a.EmailSentAt)
            .HasColumnName("email_sent_at");

        // ── Relationships ────────────────────────────────────────────────────
        builder.HasOne(a => a.StudentPackage)
            .WithMany()
            .HasForeignKey(a => a.StudentPackageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
