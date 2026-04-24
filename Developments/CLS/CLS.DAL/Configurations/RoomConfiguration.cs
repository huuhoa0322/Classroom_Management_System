using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CLS.DAL.Entities;

namespace CLS.DAL.Configurations;

public class RoomConfiguration : BaseEntityConfiguration<Room>
{
    public override void Configure(EntityTypeBuilder<Room> builder)
    {
        base.Configure(builder);

        builder.ToTable("rooms");

        builder.Property(x => x.Name)
               .HasColumnName("name")
               .HasMaxLength(255)
               .IsRequired();

        builder.Property(x => x.Capacity)
               .HasColumnName("capacity")
               .IsRequired();

        builder.Property(x => x.Status)
               .HasColumnName("status")
               .HasMaxLength(20)
               .HasDefaultValue("active")
               .IsRequired();

        // Indexes
        builder.HasIndex(x => x.Name).IsUnique();
    }
}
