import { useInfiniteQuery } from "@tanstack/react-query";
import { useIntersectionRef } from "@/shared/hooks/use-intersection-ref";
import { departmentQueryOptions } from "../api";

const PAGE_SIZE = 20;

export function usePositionsByDepartmentId(departmentId: string | null) {
  const {
    data,
    isLoading,
    isError,
    isFetchingNextPage,
    hasNextPage,
    fetchNextPage,
    refetch,
  } = useInfiniteQuery({
    ...departmentQueryOptions.getPositionsByDepartmentIdInfiniteOptions(
      departmentId ?? "",
      PAGE_SIZE,
    ),
    enabled: departmentId != null,
  });

  const cursorRef = useIntersectionRef({
    hasNextPage,
    isFetchingNextPage,
    fetchNextPage,
  });

  return {
    positions: data?.items ?? [],
    isLoading,
    isError,
    isFetchingNextPage,
    cursorRef,
    refetch,
  };
}
