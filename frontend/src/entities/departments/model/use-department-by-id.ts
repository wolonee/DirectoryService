import { departmentQueryOptions } from "@/entities/departments/api";
import { useQuery } from "@tanstack/react-query";

export function useDepartmentById(id: string | null) {
  const { data, isLoading, isError, refetch } = useQuery({
    ...departmentQueryOptions.getByIdOptions(id ?? ""),
    enabled: id != null,
  });

  return {
    department: data ?? null,
    isLoading,
    isError,
    refetch,
  };
}
