import { locationQueryOptions } from "@/entities/locations/api";
import { useIntersectionRef } from "@/shared/hooks/use-intersection-ref";
import { useInfiniteQuery } from "@tanstack/react-query";

const PAGE_SIZE = 10;
const SORT_BY = "created_at";
const SORT_DIRECTION = "desc";

export function useLocationsList({search} : {search: string}) {
    // const { data, isLoading, error, isFetching, refetch } = useQuery(
    //     locationQueryOptions.getLocationsOptions({ page, pageSize: PAGE_SIZE, sortBy: SORT_BY, sortDirection: SORT_DIRECTION })
    // );

    const { data, isLoading, error, isFetching, refetch, fetchNextPage, isFetchingNextPage, hasNextPage } = useInfiniteQuery({
        ...locationQueryOptions.getLocationsInfiniteOptions({ search: search, pageSize: PAGE_SIZE, sortBy: SORT_BY, sortDirection: SORT_DIRECTION })
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