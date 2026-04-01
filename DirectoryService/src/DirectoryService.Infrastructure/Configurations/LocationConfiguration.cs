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

        builder.ComplexProperty(v => v.LocationAddress, nb =>
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
        
        // builder.Property(ln => ln.Name)
        //     .HasConversion(v => v.Value, n => LocationName.Create(n).Value)
        //     .IsRequired(false)
        //     .HasMaxLength(LengthConstants.MAX_LENGTH_200)
        //     .HasColumnName("name");

        builder.OwnsOne(l => l.Name, nb =>
        {
            nb.Property(ln => ln.Value)
                .IsRequired()
                .HasColumnName("name")
                .HasMaxLength(LengthConstants.LENGTH200);
        });

        builder.Navigation(l => l.LocationAddress).IsRequired(false);
        
        builder.OwnsOne(l => l.Timezone, nb =>
        {
            nb.Property(tz => tz.Value)
                .IsRequired()
                .HasColumnName("timezone");
        });

        builder.Navigation(l => l.Timezone).IsRequired(false);
        
        builder.Property(l => l.IsActive).IsRequired();
    }
}