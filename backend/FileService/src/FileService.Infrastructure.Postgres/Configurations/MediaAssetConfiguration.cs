using FileService.Domain;
using FileService.Domain.Assets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileService.Infrastructure.Postgres.Configurations;

public class MediaAssetConfiguration : IEntityTypeConfiguration<MediaAsset>
{
    public void Configure(EntityTypeBuilder<MediaAsset> builder)
    {
        builder.ToTable("media_asset");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        
        builder.HasDiscriminator(x => x.AssetType)
            .HasValue<VideoAsset>(AssetType.VIDEO)
            .HasValue<PreviewAsset>(AssetType.PREVIEW);
        
        builder.Property(x => x.Usage)
            .HasColumnName("usage")
            .HasConversion<string>();

        builder.OwnsOne(m => m.MediaData, mb =>
        {
            mb.ToJson("media_data");

            mb.OwnsOne(md => md.ContentType, cb =>
            {
                cb.Property(x => x.Category).HasConversion<string>().HasColumnName("category");
                cb.Property(x => x.Value).HasColumnName("value");
            });

            mb.OwnsOne(md => md.FileName, fb =>
            {
                fb.Property(x => x.Name).HasColumnName("name");
                fb.Property(x => x.Extension).HasColumnName("extension");
            });

            mb.Property(x => x.Size).HasColumnName("size");
        });

        builder.Property(x => x.RawKey)
            .HasConversion<StorageKeyConverter>()
            .HasColumnName("raw_key")
            .HasColumnType("jsonb");

        builder.Property(x => x.FinalKey)
            .HasConversion<StorageKeyConverter>()
            .HasColumnName("final_key")
            .HasColumnType("jsonb");

        builder.Property(x => x.AssetType)
            .HasColumnName("asset_type")
            .HasConversion<string>();

        builder.OwnsOne(x => x.Owner, owner =>
        {
            owner.Property(x => x.Context)
                .HasColumnName("owner_context")
                .IsRequired();

            owner.Property(x => x.EntityId)
                .HasColumnName("owner_entity_id")
                .IsRequired();
            
            owner.Property(x => x.UploaderId)
                .HasColumnName("owner_uploader_id")
                .IsRequired();

            owner.HasIndex(x => new { x.Context, x.EntityId });
        });

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>();
        
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.OwnsOne(x => x.StorageReference, rb =>
        {
            rb.Property(x => x.Key)
                .HasConversion<StorageKeyConverter>()
                .HasColumnName("storage_reference_key")
                .HasColumnType("jsonb");

            rb.Property(x => x.ContentType)
                .HasColumnName("storage_reference_content_type");
            
            rb.Property(x => x.Size)
                .HasColumnName("storage_reference_size");
            
            rb.Property(x => x.ETag)
                .HasColumnName("storage_reference_etag");
            
            rb.Property(x => x.Checksum)
                .HasColumnName("storage_reference_checksum");
            
            rb.Property(x => x.LastModified)
                .HasColumnName("storage_reference_last_modified");
        });

        builder.Navigation(x => x.StorageReference).IsRequired(false);

        builder.HasIndex(x => new { x.Status, x.CreatedAt });
        builder.HasIndex(x => new { x.Usage, x.Status });
        builder.HasIndex(x => x.RawKey);
        builder.HasIndex(x => x.FinalKey);
    }
}
