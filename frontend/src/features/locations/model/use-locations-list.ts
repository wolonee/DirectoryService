import { locationQueryOptions } from "@/entities/locations/api";
import { useInfiniteQuery } from "@tanstack/react-query";
import { RefCallback, useCallback } from "react";

const PAGE_SIZE = 10;
const SORT_BY = "created_at";
const SORT_DIRECTION = "desc";

export function useLocationsList() {
    // const { data, isLoading, error, isFetching, refetch } = useQuery(
    //     locationQueryOptions.getLocationsOptions({ page, pageSize: PAGE_SIZE, sortBy: SORT_BY, sortDirection: SORT_DIRECTION })
    // );

    const { data, isLoading, error, isFetching, refetch, fetchNextPage, isFetchingNextPage, hasNextPage } = useInfiniteQuery({
        ...locationQueryOptions.getLocationsInfiniteOptions({ pageSize: PAGE_SIZE, sortBy: SORT_BY, sortDirection: SORT_DIRECTION })
    });

    const cursorRef: RefCallback<HTMLDivElement> = useCallback(
    (el) => {
      const observer = new IntersectionObserver(
        (entries) => {
          if (
            entries[0].isIntersecting &&
            hasNextPage &&
            !isFetchingNextPage
          ) {
            fetchNextPage();
          }
        },
        { threshold: 0.5 }
      );

      if (el) {
        observer.observe(el);

        return () => observer.disconnect();
      }
    },
    [fetchNextPage, hasNextPage, isFetchingNextPage]
  );

    return {
        locations: data?.items,
        totalCount: data?.totalCount,
        totalPages: data?.totalPages,
        page: data?.page,
        isLoading,
        error,
        isFetching,
        refetch,
        fetchNextPage,
        isFetchingNextPage,
        hasNextPage,
        cursorRef,
    };
}