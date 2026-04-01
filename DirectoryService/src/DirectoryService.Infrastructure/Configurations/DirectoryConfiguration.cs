using DirectoryService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class DirectoryConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("department");
        
        builder.HasKey(d => d.Id);

        builder.Property(d => d.DepartmentName)
            .IsRequired()
            .HasMaxLength(LengthConstants.MAX_LENGTH_150)
            .HasConversion(v => v.Value, n => new DepartmentName(n));
    }
}
