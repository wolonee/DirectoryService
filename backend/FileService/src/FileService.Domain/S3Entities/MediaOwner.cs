using CSharpFunctionalExtensions;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;

namespace FileService.Domain.S3Entities;

public sealed record MediaOwner
{
    private static readonly HashSet<string> _allowedContexts =
    [
        "lesson",
        "module",
        "user",
        "course",
        "department",
        "location",
    ];

    public string Context { get; private init; } = null!;

    public Guid EntityId { get; private init; }
    
    public Guid UploaderId { get; private init; }

    private MediaOwner()
    {
    }

    private MediaOwner(string context, Guid entityId, Guid uploaderId)
    {
        Context = context;
        EntityId = entityId;
        UploaderId = uploaderId;
    }
    
    public static Result<MediaOwner, Error> Create(string context, Guid entityId, Guid uploaderId)
    {
        if (string.IsNullOrWhiteSpace(context) || context.Length > 50)
            return GeneralErrors.ValueIsInvalid(nameof(context));

        string normalizedContext = context.Trim().ToLowerInvariant();

        if (!_allowedContexts.Contains(normalizedContext))
            return GeneralErrors.ValueIsInvalid(nameof(context));

        if (entityId == Guid.Empty)
            return GeneralErrors.ValueIsInvalid(nameof(entityId));
        
        if (uploaderId == Guid.Empty)
            return GeneralErrors.ValueIsInvalid(nameof(uploaderId));

        return new MediaOwner(normalizedContext, entityId, uploaderId);
    }
    
    public static Result<MediaOwner, Error> ForLesson(Guid lessonId, Guid uploaderId) =>
        Create("lesson", lessonId, uploaderId);

    public static Result<MediaOwner, Error> ForModule(Guid courseId, Guid uploaderId) =>
        Create("module", courseId, uploaderId);

    public static Result<MediaOwner, Error> ForUser(Guid userId, Guid uploaderId) =>
        Create("user", userId, uploaderId);

    public static Result<MediaOwner, Error> ForDepartment(Guid departmentId, Guid uploaderId) =>
        Create("department", departmentId, uploaderId);
    
    public static Result<MediaOwner, Error> ForCourse(Guid departmentId, Guid uploaderId) =>
        Create("course", departmentId, uploaderId);

    public static Result<MediaOwner, Error> ForLocation(Guid locationId, Guid uploaderId) =>
        Create("location", locationId, uploaderId);
    
}
