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

        builder.ComplexProperty(v => v.Address, nb =>
        {
            nb.Property(s => s.Street)
                .IsRequired()
                .HasMaxLength(LengthConstants.LENGTH200)
                .HasColumnName("street");

            nb.Property(s => s.City)
                .IsRequired()
                .HasMaxLength(LengthConstants.LENGTH200)
                .HasColumnName("city");

            nb.Property(s => s.Country)
                .IsRequired()
                .HasMaxLength(LengthConstants.LENGTH150)
                .HasColumnName("country");
        });

        builder.OwnsOne(l => l.Name, nb =>
        {
            nb.Property(ln => ln.Value)
                .IsRequired()
                .HasColumnName("name")
                .HasMaxLength(LengthConstants.LENGTH200);
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

        builder.HasIndex(l => l.Name.Value)
            .IsUnique()
            .HasDatabaseName("ux_locations_name");
            
        builder.HasIndex(l => l.Address.Country)
            .HasDatabaseName("ix_locations_country");
        
        builder.HasIndex(l => l.Address.Street)
            .HasDatabaseName("ix_locations_street");
        
        builder.HasIndex(l => l.Address.City)
            .HasDatabaseName("ix_locations_city");
        
        builder.HasIndex(l => new { l.Address.Country, l.Address.Street, l.Address.City })
            .IsUnique()
            .HasDatabaseName("ux_locations_full_address");

        // builder.HasIndex(l => new { l.Id, l.IsActive })  я так понял что это бесполезно
        //     .HasDatabaseName("ix_locations_active_id");  потому что Id уже уникален => у него только один IsActive
        
        builder.HasIndex(l => new { l.IsActive, l.Name.Value })
            .HasDatabaseName("ix_locations_active_name");
        
        builder.HasIndex(l => l.Timezone)
            .HasDatabaseName("ix_locations_timezone");
        
        // builder.HasIndex(l => l.CreatedAt)
        //     .HasDatabaseName("ix_locations_created_at");
        //
        // builder.HasIndex(l => l.UpdatedAt)
        //     .HasDatabaseName("ix_locations_updated_at");
    }
}