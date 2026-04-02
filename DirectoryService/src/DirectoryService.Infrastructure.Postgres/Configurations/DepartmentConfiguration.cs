using DirectoryService.Domain;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("department");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id)
            .HasColumnName("id");
        
        builder.Property(d => d.Depth)
            .IsRequired()
            .HasDefaultValue(0);
        
        builder.Property(d => d.IsActive)
            .IsRequired();

        builder.Property(d => d.DepartmentName)
            .IsRequired()
            .HasColumnName("name")
            .HasMaxLength(LengthConstants.LENGTH150)
            .HasConversion(v => v.Value, n => DepartmentName.Create(n).Value);
        
        // builder.OwnsOne(d => d.DepartmentName, db =>
        // {
        //     db.Property(d => d.Value)
        //         .IsRequired()
        //         .HasColumnName("name")
        //         .HasMaxLength(LengthConstants.MAX_LENGTH_150);
        // });
        
        builder.Property(d => d.DepartmentIdentifier)
            .IsRequired()
            .HasColumnName("identifier")
            .HasMaxLength(LengthConstants.LENGTH150)
            .HasConversion(v => v.Value, i => DepartmentIdentifier.Create(i).Value);

        builder.Property(d => d.DepartmentPath)
            .IsRequired()
            .HasMaxLength(LengthConstants.LENGTH150)
            .HasConversion(v => v.Value, p => DepartmentPath.Create(p).Value);

        // builder
        //     .HasMany(d => d.DepartmentLocations)
        //     .WithOne(l => l.Department)
        //     .HasForeignKey(l => l.DepartmentId)
        //     .IsRequired()
        //     .OnDelete(DeleteBehavior.Cascade);
        
        builder
            .HasMany(d => d.DepartmentPositions)
            .WithOne(dp => dp.Department)
            .HasForeignKey(p => p.DepartmentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
