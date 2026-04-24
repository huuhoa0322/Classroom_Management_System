using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CLS.DAL.Entities;

namespace CLS.DAL.Configurations;

public class SessionConfiguration : BaseEntityConfiguration<Session>
{
    public override void Configure(EntityTypeBuilder<Session> builder)
    {
        base.Configure(builder);

        builder.ToTable("sessions");

        builder.Property(x => x.ClassId)
               .HasColumnName("class_id")
               .IsRequired();

        builder.Property(x => x.TeacherId)
               .HasColumnName("teacher_id")
               .IsRequired();

        builder.Property(x => x.RoomId)
               .HasColumnName("room_id")
               .IsRequired();

        builder.Property(x => x.StartTime)
               .HasColumnName("start_time")
               .IsRequired();

        builder.Property(x => x.EndTime)
               .HasColumnName("end_time")
               .IsRequired();

        builder.Property(x => x.Status)
               .HasColumnName("status")
               .HasMaxLength(20)
               .HasDefaultValue("scheduled")
               .IsRequired();

        // FK: session → class (CASCADE — theo schema)
        builder.HasOne(x => x.Class)
               .WithMany(c => c.Sessions)
               .HasForeignKey(x => x.ClassId)
               .OnDelete(DeleteBehavior.Cascade);

        // FK: session → teacher (RESTRICT)
        builder.HasOne(x => x.Teacher)
               .WithMany()
               .HasForeignKey(x => x.TeacherId)
               .OnDelete(DeleteBehavior.Restrict);

        // FK: session → room (RESTRICT)
        builder.HasOne(x => x.Room)
               .WithMany()
               .HasForeignKey(x => x.RoomId)
               .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.ClassId);
        builder.HasIndex(x => x.TeacherId);
        builder.HasIndex(x => new { x.StartTime, x.EndTime });
    }
}
