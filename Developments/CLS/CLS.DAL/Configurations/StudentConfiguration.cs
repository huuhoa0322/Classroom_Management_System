using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CLS.DAL.Entities;

namespace CLS.DAL.Configurations;

public class StudentConfiguration : BaseEntityConfiguration<Student>
{
    public override void Configure(EntityTypeBuilder<Student> builder)
    {
        base.Configure(builder);

        builder.ToTable("students");

        builder.Property(x => x.FullName)
               .HasColumnName("full_name")
               .HasMaxLength(255)
               .IsRequired();

        builder.Property(x => x.DateOfBirth)
               .HasColumnName("date_of_birth");

        builder.Property(x => x.Status)
               .HasColumnName("status")
               .HasMaxLength(20)
               .HasDefaultValue("active")
               .IsRequired();

        builder.Property(x => x.EnrolledAt)
               .HasColumnName("enrolled_at")
               .IsRequired();

        builder.Property(x => x.ParentId)
               .HasColumnName("parent_id")
               .IsRequired();

        // FK: student → parent (RESTRICT — không cascade delete)
        builder.HasOne(x => x.Parent)
               .WithMany(p => p.Students)
               .HasForeignKey(x => x.ParentId)
               .OnDelete(DeleteBehavior.Restrict);

        // Index để tìm học sinh theo phụ huynh nhanh hơn
        builder.HasIndex(x => x.ParentId);
        builder.HasIndex(x => x.Status);
    }
}
