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
    }   
}