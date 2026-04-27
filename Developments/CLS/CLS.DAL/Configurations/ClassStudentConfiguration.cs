using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CLS.DAL.Entities;

namespace CLS.DAL.Configurations;

public class ClassStudentConfiguration : BaseEntityConfiguration<ClassStudent>
{
    public override void Configure(EntityTypeBuilder<ClassStudent> builder)
    {
        base.Configure(builder);

        builder.ToTable("class_students");

        builder.Property(x => x.ClassId)
               .HasColumnName("class_id")
               .IsRequired();

        builder.Property(x => x.StudentId)
               .HasColumnName("student_id")
               .IsRequired();

        builder.Property(x => x.EnrollmentDate)
               .HasColumnName("enrollment_date")
               .IsRequired();

        builder.Property(x => x.Status)
               .HasColumnName("status")
               .HasMaxLength(20)
               .HasDefaultValue("active")
               .IsRequired();

        // FK: class_student → class (RESTRICT)
        builder.HasOne(x => x.Class)
               .WithMany(c => c.ClassStudents)
               .HasForeignKey(x => x.ClassId)
               .OnDelete(DeleteBehavior.Restrict);

        // FK: class_student → student (RESTRICT)
        builder.HasOne(x => x.Student)
               .WithMany()
               .HasForeignKey(x => x.StudentId)
               .OnDelete(DeleteBehavior.Restrict);

        // UNIQUE constraint: 1 student per class
        builder.HasIndex(x => new { x.ClassId, x.StudentId })
               .IsUnique();
    }
}
