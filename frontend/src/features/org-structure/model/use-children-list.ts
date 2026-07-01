import { departmentQueryOptions } from "@/entities/departments/api";
import { useQuery } from "@tanstack/react-query";

export function useChildrenList(parentId: string, isExpanded: boolean) {
    const { data, isLoading, isError, refetch } = useQuery({
        ...departmentQueryOptions.getChildrenOptions(parentId),
        enabled: isExpanded,
    });

    return {
        children: data?.items ?? [],
        isLoading,
        isError,
        refetch,
    };
}