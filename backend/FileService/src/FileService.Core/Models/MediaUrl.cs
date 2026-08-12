using FileService.Domain;
using FileService.Domain.S3Entities;

namespace FileService.Core.Models;

public record MediaUrl(StorageKey StorageKey, string PresignedUrl);