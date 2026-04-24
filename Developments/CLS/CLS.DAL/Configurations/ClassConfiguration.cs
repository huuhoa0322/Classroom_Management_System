using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CLS.DAL.Entities;

namespace CLS.DAL.Configurations;

public class ClassConfiguration : BaseEntityConfiguration<Class>
{
    public override void Configure(EntityTypeBuilder<Class> builder)
    {
        base.Configure(builder);

        builder.ToTable("classes");

        builder.Property(x => x.Name)
               .HasColumnName("name")
               .HasMaxLength(255)
               .IsRequired();

        builder.Property(x => x.Status)
               .HasColumnName("status")
               .HasMaxLength(20)
               .HasDefaultValue("active")
               .IsRequired();

        builder.Property(x => x.CreatedBy)
               .HasColumnName("created_by")
               .IsRequired();

        // FK: class → user/admin (RESTRICT)
        builder.HasOne(x => x.Creator)
               .WithMany()
               .HasForeignKey(x => x.CreatedBy)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
