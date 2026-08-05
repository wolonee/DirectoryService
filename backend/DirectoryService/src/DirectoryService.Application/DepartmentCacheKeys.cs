namespace DirectoryService.Application;

public static class DepartmentCacheKeys
{
    public static string ChildrenPage(Guid parentId, int page, int pageSize)
        => $"dept:{parentId}:children:p{page}:s{pageSize}";

    public static string ChildrenTag(Guid parentId)
        => $"dept:{parentId}:children";
}