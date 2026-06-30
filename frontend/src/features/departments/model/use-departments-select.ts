import { departmentQueryOptions } from "@/entities/departments/api";
import { useIntersectionRef } from "@/shared/hooks/use-intersection-ref";
import { useInfiniteQuery } from "@tanstack/react-query";

const PAGE_SIZE = 20;

type Props = {
  parentId?: string;
};

export function useDepartmentsSelect({ parentId }: Props = {}) {
  const { data, isLoading, isFetchingNextPage, hasNextPage, fetchNextPage } =
    useInfiniteQuery(
      departmentQueryOptions.getInfiniteOptions({ pageSize: PAGE_SIZE, parentId: parentId }),
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
