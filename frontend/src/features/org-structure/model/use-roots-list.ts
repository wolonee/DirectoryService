import { departmentQueryOptions } from "@/entities/departments/api";
import { useQuery } from "@tanstack/react-query";

export function useRootsList() {
  const { data, isLoading, isError } = useQuery(
    departmentQueryOptions.getRootsOptions(),
  );

  return {
    roots: data?.items ?? [],
    isLoading,
    isError,
  };
}