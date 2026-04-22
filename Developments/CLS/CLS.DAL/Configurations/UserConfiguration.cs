using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CLS.DAL.Entities;

namespace CLS.DAL.Configurations;

public class UserConfiguration : BaseEntityConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        // Kế thừa cấu hình chuẩn: Id, audit fields, snake_case mapping, HasQueryFilter(!IsDeleted)
        base.Configure(builder);

        // Map tên bảng
        builder.ToTable("users");

        // Ràng buộc (Constraints) & Properties
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

        builder.Property(x => x.Role)
               .HasColumnName("role")
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(x => x.PasswordHash)
               .HasColumnName("password_hash")
               .HasMaxLength(255)
               .IsRequired();

        builder.Property(x => x.Status)
               .HasColumnName("status")
               .HasMaxLength(20)
               .HasDefaultValue("active")
               .IsRequired();

        // Indexes
        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.Role);
    }
}
