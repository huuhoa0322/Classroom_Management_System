using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CLS.DAL.Entities;

namespace CLS.DAL.Configurations;

public class ParentConfiguration : BaseEntityConfiguration<Parent>
{
    public override void Configure(EntityTypeBuilder<Parent> builder)
    {
        base.Configure(builder);

        builder.ToTable("parents");

        builder.Property(x => x.FullName)
               .HasColumnName("full_name")
               .HasMaxLength(255)
               .IsRequired();

        builder.Property(x => x.Email)
               .HasColumnName("email")
               .HasMaxLength(255)
               .IsRequired();

        builder.Property(x => x.Phone)
               .HasColumnName("phone")
               .HasMaxLength(20);

        builder.Property(x => x.Relationship)
               .HasColumnName("relationship")
               .HasMaxLength(50)
               .IsRequired();

        // Email phụ huynh là duy nhất — kênh thông báo không được trùng
        builder.HasIndex(x => x.Email).IsUnique();
    }
}
