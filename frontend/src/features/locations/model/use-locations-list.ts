import { locationQueryOptions } from "@/entities/locations/api";
import { useIntersectionRef } from "@/shared/hooks/use-intersection-ref";
import { useInfiniteQuery } from "@tanstack/react-query";
import { LocationFilterState } from "./locations-filter-store";

export function useLocationsList({search, isActive, sortBy, sortDirection, pageSize} : LocationFilterState) {
    // const { data, isLoading, error, isFetching, refetch } = useQuery(
    //     locationQueryOptions.getLocationsOptions({ page, pageSize: PAGE_SIZE, sortBy: SORT_BY, sortDirection: SORT_DIRECTION })
    // );

    const { data, isLoading, error, isFetching, refetch, fetchNextPage, isFetchingNextPage, hasNextPage } = useInfiniteQuery({
        ...locationQueryOptions.getLocationsInfiniteOptions({ search: search, isActive: isActive, sortBy: sortBy, sortDirection: sortDirection, pageSize: pageSize })
    });

    const cursorRef = useIntersectionRef({hasNextPage, isFetchingNextPage, fetchNextPage})

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