using FileService.Domain.S3Entities.Assets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileService.Infrastructure.Postgres.Configurations;

public sealed class VideoAssetConfiguration : IEntityTypeConfiguration<VideoAsset>
{
    public void Configure(EntityTypeBuilder<VideoAsset> builder)
    {
        builder.Property(x => x.HlsRootKey)
            .HasConversion<StorageKeyConverter>()
            .HasColumnName("hls_root_key")
            .HasColumnType("jsonb");

        builder.HasIndex(x => x.HlsRootKey);

        // FS-11: VideoMetadata из ffprobe хранится как единый jsonb-столбец "metadata".
        builder.OwnsOne(x => x.Metadata, md =>
        {
            md.ToJson("metadata");

            md.Property(x => x.Duration)
                .HasColumnName("duration");

            md.Property(x => x.Height)
                .HasColumnName("height");
            
            md.Property(x => x.Width)
                .HasColumnName("width");
        });
    }
}
