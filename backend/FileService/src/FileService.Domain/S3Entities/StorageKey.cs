using CSharpFunctionalExtensions;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;

namespace FileService.Domain;

public sealed record StorageKey
{
    public static StorageKey None { get; } = new(string.Empty, string.Empty, string.Empty);

    public string Key { get; }

    public string Prefix { get; }

    public string Bucket { get; }

    public string Value { get; }

    public string FullPath { get; }

    private StorageKey(string bucket, string prefix, string key)
    {
        Bucket = bucket;
        Prefix = prefix;
        Key = key;

        Value = string.IsNullOrEmpty(Prefix)
            ? Key
            : $"{Prefix}/{Key}";

        FullPath = string.IsNullOrEmpty(Bucket)
            ? string.Empty
            : $"{Bucket}/{Value}";
    }
    
    public static Result<StorageKey, Error> Create(string bucket, string? prefix, string key)
    { 
        if (string.IsNullOrWhiteSpace(bucket))
            return GeneralErrors.ValueIsInvalid("bucket");

        Result<string, Error> normalizedKeyResult = NormalizeSegment(key);
        if (normalizedKeyResult.IsFailure)
            return normalizedKeyResult.Error;

        Result<string, Error> normalizedPrefixResult = NormalizePrefix(prefix);
        if (normalizedPrefixResult.IsFailure)
            return normalizedPrefixResult.Error;

        return new StorageKey(
            bucket.Trim(),
            normalizedPrefixResult.Value,
            normalizedKeyResult.Value);
    }

    public Result<StorageKey, Error> AppendSegment(string segment)
    {
        if (string.IsNullOrEmpty(Bucket))
            return GeneralErrors.ValueIsInvalid("bucket");

        return Create(Bucket, Value, segment);
    }
    
    private static Result<string, Error> NormalizePrefix(string? prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
            return string.Empty;

        string[] parts = prefix
            .Trim()
            .Replace('\\', '/')
            .Split(
                '/',
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries);

        List<string> normalizedParts = [];

        foreach (string part in parts)
        {
            Result<string, Error> normalizedPart = NormalizeSegment(part);

            if (normalizedPart.IsFailure)
                return normalizedPart;

            if (!string.IsNullOrEmpty(normalizedPart.Value))
                normalizedParts.Add(normalizedPart.Value);
        }

        return string.Join('/', normalizedParts);
    }
    
    private static Result<string, Error> NormalizeSegment(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GeneralErrors.ValueIsInvalid("key");

        string trimmed = value.Trim();

        if (trimmed is "." or ".." ||
            trimmed.Contains('/', StringComparison.Ordinal) ||
            trimmed.Contains('\\', StringComparison.Ordinal))
        {
            return GeneralErrors.ValueIsInvalid("key");
        }

        return trimmed;
    }
}
