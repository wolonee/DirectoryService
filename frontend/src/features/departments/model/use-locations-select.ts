import { locationQueryOptions } from "@/entities/locations/api";
import { useIntersectionRef } from "@/shared/hooks/use-intersection-ref";
import { useInfiniteQuery } from "@tanstack/react-query";

const PAGE_SIZE = 20;

export function useLocationsSelect() {
  const { data, isLoading, isFetchingNextPage, hasNextPage, fetchNextPage } =
    useInfiniteQuery(locationQueryOptions.getInfiniteOptions({ pageSize: PAGE_SIZE }));

  const cursorRef = useIntersectionRef({ hasNextPage, isFetchingNextPage, fetchNextPage });

  return {
    locations: data?.items ?? [],
    isLoading,
    isFetchingNextPage,
    cursorRef,
  };
}
