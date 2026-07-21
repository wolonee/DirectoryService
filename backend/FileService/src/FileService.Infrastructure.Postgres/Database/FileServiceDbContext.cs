using FileService.Domain.Assets;
using Microsoft.EntityFrameworkCore;

namespace FileService.Infrastructure.Postgres.Database;

public class FileServiceDbContext : DbContext
{
    private readonly string _connectionString;
    
    public FileServiceDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();

    public DbSet<VideoAsset> VideoAssets => Set<VideoAsset>();

    public DbSet<PreviewAsset> PreviewAssets => Set<PreviewAsset>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_connectionString);
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("files");

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(FileServiceDbContext).Assembly);
    }
}
