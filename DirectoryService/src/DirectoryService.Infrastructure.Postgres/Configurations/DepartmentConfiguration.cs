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

        builder.Property(d => d.DepartmentName)
            .IsRequired()
            .HasColumnName("name")
            .HasMaxLength(LengthConstants.LENGTH150)
            .HasConversion(v => v.Value, n => DepartmentName.Create(n).Value);
        
        builder.OwnsOne(d => d.DepartmentIdentifier, identifier =>
        {
            identifier.Property(i => i.Value)
                .IsRequired()
                .HasColumnName("identifier")
                .HasMaxLength(LengthConstants.LENGTH150);
            
            // identifier.HasIndex(i => i.Value)
            //     .IsUnique()
            //     .HasDatabaseName("IX_Departments_Identifier_Unique");
        });
        
        builder.ComplexProperty(d => d.DepartmentPath, db =>
        {
            db.Property(d => d.Value)
                .IsRequired()
                .HasColumnName("path")
                .HasMaxLength(LengthConstants.LENGTH150);
        });
        
        builder.Property(d => d.Depth)
            .IsRequired()
            .HasColumnName("depth")
            .HasDefaultValue(0);
        
        builder.Property(d => d.IsActive)
            .HasColumnName("is_active")
            .IsRequired();
        
        builder.Property(d => d.ChildrenCount)
            .HasColumnName("children_count")
            .IsRequired();

        builder.Property(d => d.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
        
        builder.Property(d => d.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
        
        builder
            .HasMany(d => d.DepartmentPositions)
            .WithOne(dp => dp.Department)
            .HasForeignKey(p => p.DepartmentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
        builder
            .HasMany(d => d.ChildrenDepartments)
            .WithOne()
            .HasForeignKey(x => x.ParentId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        
        // indexes

        builder.HasIndex(d => d.DepartmentName.Value)
            .HasDatabaseName("ix_departments_name");
        
        builder.HasIndex(d => d.DepartmentIdentifier.Value)
            .IsUnique()
            .HasDatabaseName("ux_departments_identifier");
        
        builder.HasIndex(d => d.DepartmentPath.Value)
            .HasDatabaseName("ix_departments_path");

        builder.HasIndex(d => d.ParentId)
            .HasDatabaseName("ix_parent_id");

        // builder.HasIndex(d => new { d.Id, d.IsActive });
        // builder.HasIndex(d => d.CreatedAt)
        //     .HasDatabaseName("ix_created_at");
        //
        // builder.HasIndex(d => d.UpdatedAt)
        //     .HasDatabaseName("ix_updated_at");
    }
}
