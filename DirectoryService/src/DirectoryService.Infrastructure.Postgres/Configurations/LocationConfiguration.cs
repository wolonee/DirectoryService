using DirectoryService.Domain;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("locations");
        
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id)
            .HasColumnName("id");

        builder.OwnsOne(v => v.Address, address =>
        {
            address.Property(s => s.Country)
                .IsRequired()
                .HasMaxLength(LengthConstants.LENGTH150)
                .HasColumnName("country");
            
            address.HasIndex(l => l.Country)
                .HasDatabaseName("ix_locations_country");
            
            address.Property(s => s.City)
                .IsRequired()
                .HasMaxLength(LengthConstants.LENGTH200)
                .HasColumnName("city");
            
            address.HasIndex(l => l.City)
                .HasDatabaseName("ix_locations_city");
            
            address.Property(s => s.Street)
                .IsRequired()
                .HasMaxLength(LengthConstants.LENGTH200)
                .HasColumnName("street");
        
            address.HasIndex(l => l.Street)
                .HasDatabaseName("ix_locations_street");
            
            address.HasIndex(l => new { l.Country, l.Street, l.City })
                .IsUnique()
                .HasDatabaseName("ux_locations_full_address");
        });

        builder.OwnsOne(l => l.Name, name =>
        {
            name.Property(ln => ln.Value)
                .IsRequired()
                .HasColumnName("name")
                .HasMaxLength(LengthConstants.LENGTH200);
            
            name.HasIndex(l => l.Value)
                .IsUnique()
                .HasDatabaseName("ux_locations_name");
        });
        builder.Navigation(l => l.Name).IsRequired(false);
        
        builder.Property(l => l.Timezone)
            .HasConversion(v => v.Value, tz => LocationTimeZone.Create(tz).Value)
            .IsRequired()
            .HasMaxLength(LengthConstants.LENGTH200)
            .HasColumnName("timezone");

        builder.Property(l => l.IsActive)
            .IsRequired()
            .HasColumnName("is_active");
        
        // indexes
        
        // builder.HasIndex(l => new { l.Id, l.IsActive })  я так понял что это бесполезно
        //     .HasDatabaseName("ix_locations_active_id");  потому что Id уже уникален => у него только один IsActive
        
        builder.HasIndex(l => l.Timezone)
            .HasDatabaseName("ix_locations_timezone");
    }
}