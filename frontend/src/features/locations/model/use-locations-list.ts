import { locationQueryOptions } from "@/entities/locations/api";
import { useQuery } from "@tanstack/react-query";

const PAGE_SIZE = 2;
const SORT_BY = "created_at";
const SORT_DIRECTION = "desc";

export function useLocationsList({ page }: { page: number; }) {
    const { data, isLoading, error, isFetching, refetch } = useQuery(
        locationQueryOptions.getLocationsOptions({ page, pageSize: PAGE_SIZE, sortBy: SORT_BY, sortDirection: SORT_DIRECTION })
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
    };
}