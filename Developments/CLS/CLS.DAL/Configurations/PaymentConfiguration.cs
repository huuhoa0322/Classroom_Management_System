using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CLS.DAL.Entities;

namespace CLS.DAL.Configurations;

public class PaymentConfiguration : BaseEntityConfiguration<Payment>
{
    public override void Configure(EntityTypeBuilder<Payment> builder)
    {
        base.Configure(builder);

        builder.ToTable("payments");

        builder.Property(x => x.StudentPackageId)
               .HasColumnName("student_package_id")
               .IsRequired();

        builder.Property(x => x.AdminId)
               .HasColumnName("admin_id")
               .IsRequired();

        builder.Property(x => x.Amount)
               .HasColumnName("amount")
               .HasPrecision(12, 2)
               .IsRequired();

        builder.Property(x => x.PaymentDate)
               .HasColumnName("payment_date")
               .IsRequired();

        builder.Property(x => x.PaymentMethod)
               .HasColumnName("payment_method")
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(x => x.Status)
               .HasColumnName("status")
               .HasMaxLength(20)
               .HasDefaultValue("pending")
               .IsRequired();


        // FK: payment → student_package (RESTRICT)
        builder.HasOne(x => x.StudentPackage)
               .WithMany(sp => sp.Payments)
               .HasForeignKey(x => x.StudentPackageId)
               .OnDelete(DeleteBehavior.Restrict);

        // FK: payment → user/admin (RESTRICT)
        builder.HasOne(x => x.RecordedBy)
               .WithMany()
               .HasForeignKey(x => x.AdminId)
               .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.StudentPackageId);
        builder.HasIndex(x => x.Status);
    }
}
