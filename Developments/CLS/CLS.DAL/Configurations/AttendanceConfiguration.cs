using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CLS.DAL.Entities;

namespace CLS.DAL.Configurations;

public class AttendanceConfiguration : BaseEntityConfiguration<Attendance>
{
    public override void Configure(EntityTypeBuilder<Attendance> builder)
    {
        base.Configure(builder);

        builder.ToTable("attendances");

        builder.Property(x => x.SessionId)
               .HasColumnName("session_id")
               .IsRequired();

        builder.Property(x => x.StudentId)
               .HasColumnName("student_id")
               .IsRequired();

        builder.Property(x => x.Status)
               .HasColumnName("status")
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(x => x.Note)
               .HasColumnName("note");

        builder.Property(x => x.RecordedAt)
               .HasColumnName("recorded_at")
               .IsRequired();

        // FK: attendance → session (CASCADE — theo schema)
        builder.HasOne(x => x.Session)
               .WithMany()
               .HasForeignKey(x => x.SessionId)
               .OnDelete(DeleteBehavior.Cascade);

        // FK: attendance → student (RESTRICT)
        builder.HasOne(x => x.Student)
               .WithMany()
               .HasForeignKey(x => x.StudentId)
               .OnDelete(DeleteBehavior.Restrict);

        // UNIQUE constraint: 1 student per session
        builder.HasIndex(x => new { x.SessionId, x.StudentId })
               .IsUnique();

        // Performance indexes
        builder.HasIndex(x => x.SessionId);
        builder.HasIndex(x => x.RecordedAt);
    }
}
