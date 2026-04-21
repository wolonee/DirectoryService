using DirectoryService.Domain;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class DepartmentLocationConfiguration : IEntityTypeConfiguration<DepartmentLocation>
{
    public void Configure(EntityTypeBuilder<DepartmentLocation> builder)
    {
        builder.ToTable("department_locations");
        
        builder.HasKey(dl => dl.Id);
        builder.Property(dl => dl.Id)
            .IsRequired()
            .HasColumnName("id");
        
        builder.Property(dl => dl.LocationId)
            .HasColumnName("location_id")
            .IsRequired();
        
        builder.Property(dl => dl.DepartmentId)
            .HasColumnName("department_id")
            .IsRequired();
        
        builder
            .HasOne(d => d.Department)
            .WithMany(dl => dl.DepartmentLocations)
            .HasForeignKey(dl => dl.DepartmentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne<Location>()
            .WithMany()
            .HasForeignKey(dl => dl.LocationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}