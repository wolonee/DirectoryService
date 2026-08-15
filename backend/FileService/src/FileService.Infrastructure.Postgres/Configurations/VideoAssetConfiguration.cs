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

        // FS-10: VideoMetadata пока не персистится (mock, in-memory). Реальное сохранение как jsonb —
        // в FS-11 вместе с ffprobe (+ миграция). Без Ignore EF пытается замапить VO как entity и падает.
        builder.OwnsOne(x => x.Metadata).ToJson();
    }
}
