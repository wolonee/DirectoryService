using System.Text.Json;
using DirectoryService.Domain;
using DirectoryService.Domain.Departments;
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
        builder.Navigation(p => p.Name).IsRequired();


        builder.OwnsOne(d => d.Description, ob =>
        {
            ob.Property(d => d.Value)
                .HasColumnName("description")
                .HasMaxLength(LengthConstants.LENGTH1000)
                .IsRequired(false);
        });
        
        builder.Property(a => a.IsActive)
            .HasColumnName("is_active")
            .IsRequired();
        
        // indexes
        
        builder.HasIndex("((name ->> 'speciality'))")
            .HasDatabaseName("ix_position_name_speciality");

        builder.HasIndex("((name ->> 'direction'))")
            .HasDatabaseName("ix_position_name_direction");

        builder.HasIndex("((name ->> 'speciality'))", "((name ->> 'direction'))")
            .HasDatabaseName("ix_position_name_full");
        
        builder.HasIndex("is_active", "((name ->> 'speciality'))")
            .HasDatabaseName("ix_position_active_speciality");
        
        // builder.HasIndex(p => p.CreatedAt)
        //     .HasDatabaseName("ix_position_created_at");
        //     
        // builder.HasIndex(p => p.UpdatedAt)
        //     .HasDatabaseName("ix_position_updated_at");
    }
}