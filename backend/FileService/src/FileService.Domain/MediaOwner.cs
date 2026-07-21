using CSharpFunctionalExtensions;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;

namespace FileService.Domain;

public sealed record MediaOwner
{
    private static readonly HashSet<string> _allowedContexts =
    [
        "lesson",
        "module",
        "user",
        "course",
        "department",
    ];

    public string Context { get; private init; } = null!;

    public Guid EntityId { get; private init; }

    private MediaOwner()
    {
    }

    private MediaOwner(string context, Guid entityId)
    {
        Context = context;
        EntityId = entityId;
    }
    
    public static Result<MediaOwner, Error> Create(string context, Guid entityId)
    {
        if (string.IsNullOrWhiteSpace(context) || context.Length > 50)
            return GeneralErrors.ValueIsInvalid(nameof(context));

        string normalizedContext = context.Trim().ToLowerInvariant();

        if (!_allowedContexts.Contains(normalizedContext))
            return GeneralErrors.ValueIsInvalid(nameof(context));

        if (entityId == Guid.Empty)
            return GeneralErrors.ValueIsInvalid(nameof(entityId));

        return new MediaOwner(normalizedContext, entityId);
    }
    
    public static Result<MediaOwner, Error> ForLesson(Guid lessonId) =>
        Create("lesson", lessonId);

    public static Result<MediaOwner, Error> ForModule(Guid courseId) =>
        Create("module", courseId);

    public static Result<MediaOwner, Error> ForUser(Guid userId) =>
        Create("user", userId);

    public static Result<MediaOwner, Error> ForDepartment(Guid departmentId) =>
        Create("department", departmentId);
    
    public static Result<MediaOwner, Error> ForCourse(Guid departmentId) =>
        Create("course", departmentId);
    
}
