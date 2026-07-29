using Microsoft.Extensions.Options;

namespace FileService.Infrastructure.S3.UnitTests;

public class ChunkSizeCalculatorTests
{
    [Fact]
    public void CalculateChunkSize_WhenFileFitsOnePart_ReturnsOnePart()
    {
        // Arrange
        var calculator = CreateCalculator();

        // Act
        var result = calculator.CalculateChunkSize(3L * 1024 * 1024);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3L * 1024 * 1024, result.Value.ChunkSize);
        Assert.Equal(1, result.Value.TotalChunks);
    }

    [Fact]
    public void CalculateChunkSize_WhenMoreThanS3MaximumPartsAreRequested_UsesMaximumParts()
    {
        // Arrange
        var calculator = CreateCalculator(new S3Options
        {
            MinimumChunkSizeBytes = S3Options.S3MinimumPartSizeBytes,
            RecommendedChunkSizeBytes = S3Options.S3MinimumPartSizeBytes,
            MaxChunks = S3Options.S3MaximumPartsCount,
        });

        // Act
        var result = calculator.CalculateChunkSize(
            S3Options.S3MinimumPartSizeBytes * (S3Options.S3MaximumPartsCount + 1L));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(S3Options.S3MaximumPartsCount, result.Value.TotalChunks);
        Assert.True(result.Value.ChunkSize >= S3Options.S3MinimumPartSizeBytes);
    }

    [Fact]
    public void CalculateChunkSize_WhenConfiguredMinimumIsBelowS3Limit_ReturnsFailure()
    {
        // Arrange
        var calculator = CreateCalculator(new S3Options
        {
            MinimumChunkSizeBytes = S3Options.S3MinimumPartSizeBytes - 1,
        });

        // Act
        var result = calculator.CalculateChunkSize(100L * 1024 * 1024);

        // Assert
        Assert.True(result.IsFailure);
    }

    private static ChunkSizeCalculator CreateCalculator(S3Options? options = null) =>
        new(Options.Create(options ?? new S3Options
        {
            MinimumChunkSizeBytes = S3Options.S3MinimumPartSizeBytes,
            RecommendedChunkSizeBytes = 100L * 1024 * 1024,
            MaxChunks = S3Options.S3MaximumPartsCount,
        }));
}
