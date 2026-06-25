import { apiClient } from "@/shared/api/axios-instance";
import type {
  CreateLocationRequest,
  GetLocationDto,
  GetLocationsRequest,
  GetLocationsResponse,
} from "./types";
import type { Envelope } from "@/shared/api/types/envelope";
import type { PaginationResponse } from "@/shared/api/types/pagination";
import { infiniteQueryOptions, queryOptions } from "@tanstack/react-query";

export const locationsApi = {
  getLocations: async (
    request: GetLocationsRequest,
  ): Promise<PaginationResponse<GetLocationDto>> => {
    const response = await apiClient.get<Envelope<GetLocationsResponse>>(
      "/locations",
      {
        params: {
          Search: request.search,
          SortBy: request.sortBy,
          SortDirection: request.sortDirection,
          "Pagination.Page": request.pagination?.page,
          "Pagination.PageSize": request.pagination?.pageSize,
        },
      },
    );

    const result = response.data.result;

    if (!result) {
      return {
        items: [],
        totalCount: 0,
        page: request.pagination?.page ?? 1,
        pageSize: request.pagination?.pageSize ?? 20,
        totalPages: 0,
      };
    }

    return {
      items: result.locations,
      totalCount: result.totalCount,
      page: result.page,
      pageSize: result.pageSize,
      totalPages: result.totalPages,
    };
  },

  createLocation: async (request: CreateLocationRequest) => {
    const response = await apiClient.post<Envelope<string>>(
      "/locations",
      request,
    );

    return response.data;
  },
};

export const locationQueryOptions = {
  baseKey: "locations",

  getLocationsOptions: ({
    page,
    pageSize,
    sortBy,
    sortDirection,
  }: {
    page: number;
    pageSize: number;
    sortBy: string;
    sortDirection: string;
  }) => {
    return queryOptions({
      queryFn: () =>
        locationsApi.getLocations({
          pagination: { page, pageSize },
          sortBy,
          sortDirection,
        }),
      queryKey: [locationQueryOptions.baseKey, { page, sortBy, sortDirection }],
    });
  },

  getLocationsInfiniteOptions: ({
    pageSize,
    sortBy,
    sortDirection,
  }: {
    pageSize: number;
    sortBy: string;
    sortDirection: string;
  }) => {
    return infiniteQueryOptions({
      queryKey: [
        locationQueryOptions.baseKey,
        "infinite",
        { pageSize, sortBy, sortDirection },
      ],
      queryFn: ({ pageParam }) => {
        return locationsApi.getLocations({
          pagination: { page: pageParam, pageSize },
          sortBy,
          sortDirection,
        });
      },
      initialPageParam: 1,
      getNextPageParam: (response) => {
        if (!response || response.page >= response.totalPages) return undefined;
        return response.page + 1;
      },
      select: (data): PaginationResponse<GetLocationDto> => ({
        items: data.pages.flatMap((page) => page?.items ?? []),
        totalCount: data.pages[0]?.totalCount ?? 0,
        page: data.pages[0]?.page ?? 1,
        pageSize: data.pages[0]?.pageSize ?? pageSize,
        totalPages: data.pages[0]?.totalPages ?? 0,
      }),
    });
  },
};
