using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CLS.DAL.Entities;

namespace CLS.DAL.Configurations;

public class StudentPackageConfiguration : BaseEntityConfiguration<StudentPackage>
{
    public override void Configure(EntityTypeBuilder<StudentPackage> builder)
    {
        base.Configure(builder);

        builder.ToTable("student_packages");

        builder.Property(x => x.StudentId)
               .HasColumnName("student_id")
               .IsRequired();

        builder.Property(x => x.PackageId)
               .HasColumnName("package_id")
               .IsRequired();

        builder.Property(x => x.StartDate)
               .HasColumnName("start_date")
               .IsRequired();

        builder.Property(x => x.EndDate)
               .HasColumnName("end_date")
               .IsRequired();

        builder.Property(x => x.TotalSessions)
               .HasColumnName("total_sessions")
               .IsRequired();

        builder.Property(x => x.RemainingSessions)
               .HasColumnName("remaining_sessions")
               .IsRequired();

        builder.Property(x => x.Status)
               .HasColumnName("status")
               .HasMaxLength(20)
               .HasDefaultValue("active")
               .IsRequired();

        // FK: student_package → student (RESTRICT)
        builder.HasOne(x => x.Student)
               .WithMany(s => s.StudentPackages)
               .HasForeignKey(x => x.StudentId)
               .OnDelete(DeleteBehavior.Restrict);

        // FK: student_package → package (RESTRICT)
        builder.HasOne(x => x.Package)
               .WithMany(p => p.StudentPackages)
               .HasForeignKey(x => x.PackageId)
               .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.StudentId);
        builder.HasIndex(x => x.Status);
    }
}
