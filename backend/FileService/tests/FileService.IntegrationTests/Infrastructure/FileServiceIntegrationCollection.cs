namespace FileService.IntegrationTests.Infrastructure;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class FileServiceIntegrationCollection : ICollectionFixture<FileServiceTestWebFactory>
{
    public const string Name = "FileService integration";
}
