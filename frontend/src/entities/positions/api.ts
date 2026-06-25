import { PaginationResponse } from "@/shared/api/types/pagination";
import {
  CreatePositionRequest,
  GetPositionDto,
  GetPositionsRequest,
  GetPositionsResponse,
} from "./types";
import { apiClient } from "@/shared/api/axios-instance";
import { Envelope } from "@/shared/api/types/envelope";
import { infiniteQueryOptions } from "@tanstack/react-query";

export const positionApi = {
  getPositions: async (
    request: GetPositionsRequest,
  ): Promise<PaginationResponse<GetPositionDto>> => {
    const response = await apiClient.get<Envelope<GetPositionsResponse>>(
      "/positions",
      {
        params: {
          Search: request.search,
          SortBy: request.sortBy,
          SortDir: request.sortDirection,
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
      items: result.positions,
      totalCount: result.totalCount,
      page: result.page,
      pageSize: result.pageSize,
      totalPages: result.totalPages,
    };
  },

  createPosition: async (request: CreatePositionRequest) => {
    const response = await apiClient.post<Envelope<string>>(
      "/positions",
      request,
    );

    return response.data.result;
  },
};

export const positionQueryOptions = {
  baseKey: "positions",

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
        positionQueryOptions.baseKey,
        "infinite",
        { pageSize, sortBy, sortDirection },
      ],
      queryFn: ({ pageParam }) =>
        positionApi.getPositions({
          pagination: { page: pageParam, pageSize },
          sortBy,
          sortDirection,
        }),
      initialPageParam: 1,
      getNextPageParam: (response) => {
        if (!response || response.page >= response.totalPages) return undefined;
        return response.page + 1;
      },
      select: (data): PaginationResponse<GetPositionDto> => {
        const seen = new Set<string>();
        const items = data.pages
          .flatMap((page) => page?.items ?? [])
          .filter((item) => {
            if (seen.has(item.id)) return false;
            seen.add(item.id);
            return true;
          });
        return {
          items,
          totalCount: data.pages[0]?.totalCount ?? 0,
          page: data.pages[0]?.page ?? 1,
          pageSize: data.pages[0]?.pageSize ?? pageSize,
          totalPages: data.pages[0]?.totalPages ?? 0,
        };
      },
    });
  },
};
