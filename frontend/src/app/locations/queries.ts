import { locationsApi } from "@/entities/locations/api";
import { keepPreviousData } from "@tanstack/react-query";


export function getLocationsQueryOptions(page: number, pageSize: number) {
    return {
        queryKey: ["locations", { page }],
        queryFn: () => locationsApi.getLocations({
            pagination: {
                page,
                pageSize,
            }
        }),
        placeholderData: keepPreviousData,
    }
}