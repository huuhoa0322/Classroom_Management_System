using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CLS.DAL.Entities;

namespace CLS.DAL.Configurations;

public class FeedbackConfiguration : BaseEntityConfiguration<Feedback>
{
    public override void Configure(EntityTypeBuilder<Feedback> builder)
    {
        base.Configure(builder);

        builder.ToTable("feedbacks");

        builder.Property(x => x.SessionId)
               .HasColumnName("session_id")
               .IsRequired();

        builder.Property(x => x.StudentId)
               .HasColumnName("student_id")
               .IsRequired();

        builder.Property(x => x.TeacherId)
               .HasColumnName("teacher_id")
               .IsRequired();

        builder.Property(x => x.Content)
               .HasColumnName("content")
               .IsRequired();

        builder.Property(x => x.Score)
               .HasColumnName("score");

        builder.Property(x => x.SubmittedAt)
               .HasColumnName("submitted_at")
               .IsRequired();

        builder.Property(x => x.IsSlaOverdue)
               .HasColumnName("is_sla_overdue")
               .HasDefaultValue(false)
               .IsRequired();

        // FK: feedback → session (CASCADE)
        builder.HasOne(x => x.Session)
               .WithMany()
               .HasForeignKey(x => x.SessionId)
               .OnDelete(DeleteBehavior.Cascade);

        // FK: feedback → student (CASCADE)
        builder.HasOne(x => x.Student)
               .WithMany()
               .HasForeignKey(x => x.StudentId)
               .OnDelete(DeleteBehavior.Cascade);

        // FK: feedback → teacher/user (RESTRICT)
        builder.HasOne(x => x.Teacher)
               .WithMany()
               .HasForeignKey(x => x.TeacherId)
               .OnDelete(DeleteBehavior.Restrict);

        // Performance indexes
        builder.HasIndex(x => x.SessionId);
        builder.HasIndex(x => x.TeacherId);
    }
}
