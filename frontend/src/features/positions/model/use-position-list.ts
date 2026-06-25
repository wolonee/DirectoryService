import { positionQueryOptions } from "@/entities/positions/api";
import { useIntersectionRef } from "@/shared/hooks/use-intersection-ref";
import { useInfiniteQuery } from "@tanstack/react-query";

const PAGE_SIZE = 10;
const SORT_BY = "created_at";
const SORT_DIRECTION = "desc";

//   search?: string;
//   sortBy?: string;
//   sortDir?: string;
//   pagination?: PaginationRequest;

export function usePositionsList() {
    const { data, isLoading, error, isFetchingNextPage, hasNextPage, fetchNextPage, refetch, isFetching } = useInfiniteQuery(
        positionQueryOptions.getLocationsInfiniteOptions({ pageSize: PAGE_SIZE, sortBy: SORT_BY, sortDirection: SORT_DIRECTION })
    )

    const cursorRef = useIntersectionRef({hasNextPage, isFetchingNextPage, fetchNextPage})
    

    return {
        positions: data?.items,
        totalCount: data?.totalCount,
        isLoading: isLoading,
        error: error,
        isFetchingNextPage: isFetchingNextPage,
        cursorRef: cursorRef,
        refetch: refetch,
        isFetching: isFetching
    }
}