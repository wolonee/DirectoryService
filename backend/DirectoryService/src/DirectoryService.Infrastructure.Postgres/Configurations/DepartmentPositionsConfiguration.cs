using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class DepartmentPositionsConfiguration : IEntityTypeConfiguration<DepartmentPosition>
{
    public void Configure(EntityTypeBuilder<DepartmentPosition> builder)
    {
        builder.ToTable("department_positions");
        
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .IsRequired();
        
        builder.Property(x => x.DepartmentId)
            .HasColumnName("department_id")
            .IsRequired();
        
        builder.Property(x => x.PositionId)
            .HasColumnName("position_id")
            .IsRequired();
        
        builder
            .HasOne(d => d.Department)
            .WithMany(dp => dp.DepartmentPositions)
            .HasForeignKey(d => d.DepartmentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
        builder
            .HasOne<Position>()
            .WithMany(p => p.DepartmentPositions)
            .HasForeignKey(d => d.PositionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        
        // indexes
        
        builder.HasIndex(dp => new { dp.DepartmentId, dp.PositionId })
            .IsUnique()
            .HasDatabaseName("ix_department_positions_department_id_position_id");

        builder.HasIndex(dl => dl.DepartmentId)
            .HasDatabaseName("ix_department_positions_department_id");
        
        builder.HasIndex(dl => dl.PositionId)
            .HasDatabaseName("ix_department_positions_position_id");
    }   
}