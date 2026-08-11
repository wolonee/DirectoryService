using System.Text.Json;
using FileService.Domain;
using FileService.Domain.S3Entities;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FileService.Infrastructure.Postgres.Configurations;

public sealed class StorageKeyConverter : ValueConverter<StorageKey, string>
{
    public StorageKeyConverter()
        : base(
            storageKey => Serialize(storageKey),
            value => Deserialize(value))
    {
    }

    private static string Serialize(StorageKey storageKey) =>
        JsonSerializer.Serialize(new StorageKeyPayload(
            storageKey.Bucket,
            storageKey.Prefix,
            storageKey.Key));

    private static StorageKey Deserialize(string value)
    {
        StorageKeyPayload payload = JsonSerializer.Deserialize<StorageKeyPayload>(value)
            ?? throw new InvalidOperationException("Storage key metadata is missing.");

        if (string.IsNullOrEmpty(payload.Bucket) &&
            string.IsNullOrEmpty(payload.Prefix) &&
            string.IsNullOrEmpty(payload.Key))
        {
            return StorageKey.None;
        }

        var storageKeyResult = StorageKey.Create(payload.Bucket, payload.Prefix, payload.Key);
        if (storageKeyResult.IsFailure)
        {
            throw new InvalidOperationException(
                $"Stored storage key is invalid: {storageKeyResult.Error.Message}");
        }

        return storageKeyResult.Value;
    }

    private sealed record StorageKeyPayload(string Bucket, string Prefix, string Key);
}
