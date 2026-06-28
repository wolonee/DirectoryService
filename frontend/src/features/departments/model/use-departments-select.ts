import { departmentQueryOptions } from "@/entities/departments/api";
import { useIntersectionRef } from "@/shared/hooks/use-intersection-ref";
import { useInfiniteQuery } from "@tanstack/react-query";

const PAGE_SIZE = 20;

export function useDepartmentsSelect() {
  const { data, isLoading, isFetchingNextPage, hasNextPage, fetchNextPage } =
    useInfiniteQuery(
      departmentQueryOptions.getInfiniteOptions({ pageSize: PAGE_SIZE }),
    );

  const cursorRef = useIntersectionRef({
    hasNextPage,
    isFetchingNextPage,
    fetchNextPage,
  });

  return {
    departments: data?.items ?? [],
    isLoading,
    isFetchingNextPage,
    cursorRef,
  };
}
