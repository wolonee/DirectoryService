using System.Text.Json;
using DirectoryService.Domain;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Positions.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("position");
        
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasColumnName("id");

        // builder.Property(p => p.Name)
        //     .HasConversion(
        //         v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
        //         json => JsonSerializer.Deserialize<PositionName>(json, JsonSerializerOptions.Default)!)
        //     .HasColumnType("jsonb");

        builder.OwnsOne(p => p.Name, ob =>
        {
            ob.ToJson("name");

            ob.Property(s => s.Speciality)
                .HasMaxLength(LengthConstants.LENGTH200)
                .HasColumnName("speciality")
                .IsRequired();

            ob.Property(d => d.Direction)
                .HasMaxLength(LengthConstants.LENGTH200)
                .HasColumnName("direction")
                .IsRequired();
        });
        
        builder.Property(d => d.Description)
            .HasMaxLength(LengthConstants.LENGTH1000)
            .HasColumnName("description")
            .IsRequired(false);
        
        builder.Property(a => a.IsActive)
            .HasColumnName("isActive")
            .IsRequired();
    }
}