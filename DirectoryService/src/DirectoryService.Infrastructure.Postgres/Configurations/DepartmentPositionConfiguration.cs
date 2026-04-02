using DirectoryService.Domain.Departments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class DepartmentPositionConfiguration : IEntityTypeConfiguration<DepartmentPosition>
{
    public void Configure(EntityTypeBuilder<DepartmentPosition> builder)
    {
        builder.ToTable("department_position");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .IsRequired();
        
        builder.Property(x => x.DepartmentId)
            .IsRequired();
        
        builder.Property(x => x.PositionId)
            .IsRequired();
        
        // builder
        //     .HasOne(d => d.Department)
        //     .WithMany()
        //     .HasForeignKey(d => d.DepartmentId)
        //     .IsRequired()
        //     .OnDelete(DeleteBehavior.Cascade);
    }
}