import {
  DepartmentsListFilter,
  departmentQueryOptions,
} from "@/entities/departments/api";
import { useIntersectionRef } from "@/shared/hooks/use-intersection-ref";
import { useInfiniteQuery } from "@tanstack/react-query";

const PAGE_SIZE = 10;

type Params = Omit<DepartmentsListFilter, "pageSize">;

export function useDepartmentsList({ search, isActive, sortBy, sortDir }: Params) {
  const {
    data,
    isLoading,
    error,
    isFetching,
    refetch,
    fetchNextPage,
    isFetchingNextPage,
    hasNextPage,
  } = useInfiniteQuery(
    departmentQueryOptions.getListInfiniteOptions({
      search,
      isActive,
      sortBy,
      sortDir,
      pageSize: PAGE_SIZE,
    }),
  );

  const cursorRef = useIntersectionRef({
    hasNextPage,
    isFetchingNextPage,
    fetchNextPage,
  });

  return {
    departments: data?.items,
    totalCount: data?.totalCount,
    isLoading,
    error,
    isFetching,
    refetch,
    isFetchingNextPage,
    cursorRef,
  };
}
