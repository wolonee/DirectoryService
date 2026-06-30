import { departmentQueryOptions } from "@/entities/departments/api";
import { useIntersectionRef } from "@/shared/hooks/use-intersection-ref";
import { useInfiniteQuery } from "@tanstack/react-query";

const PAGE_SIZE = 20;

// type Props = {
//   parentId?: string;
// };

export function useDepartmentsSelect() {
  const {
    data,
    isLoading,
    isError,
    isFetchingNextPage,
    hasNextPage,
    fetchNextPage,
    refetch,
  } = useInfiniteQuery(
    departmentQueryOptions.getListInfiniteOptions({ pageSize: PAGE_SIZE }),
  );

  const cursorRef = useIntersectionRef({
    hasNextPage,
    isFetchingNextPage,
    fetchNextPage,
  });

  return {
    departments: data?.items ?? [],
    isLoading,
    isError,
    isFetchingNextPage,
    cursorRef,
    refetch,
  };
}
