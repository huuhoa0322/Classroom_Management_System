using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CLS.DAL.Entities;

namespace CLS.DAL.Configurations;

public class TuitionPackageConfiguration : BaseEntityConfiguration<TuitionPackage>
{
    public override void Configure(EntityTypeBuilder<TuitionPackage> builder)
    {
        base.Configure(builder);

        builder.ToTable("packages");

        builder.Property(x => x.Name)
               .HasColumnName("name")
               .HasMaxLength(255)
               .IsRequired();

        builder.Property(x => x.TotalSessions)
               .HasColumnName("total_sessions")
               .IsRequired();

        builder.Property(x => x.DurationDays)
               .HasColumnName("duration_days")
               .IsRequired();

        builder.Property(x => x.Price)
               .HasColumnName("price")
               .HasPrecision(12, 2)
               .IsRequired();

        builder.Property(x => x.Status)
               .HasColumnName("status")
               .HasMaxLength(20)
               .HasDefaultValue("active")
               .IsRequired();
    }
}
