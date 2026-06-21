import { apiClient } from "@/shared/api/axios-instance";
import type {
  CreateLocationRequest,
  GetLocationDto,
  GetLocationsRequest,
  GetLocationsResponse,
} from "./types";
import type { Envelope } from "@/shared/api/types/envelope";
import type { PaginationResponse } from "@/shared/api/types/pagination";

export const locationsApi = {
  getLocations: async (
    request: GetLocationsRequest): Promise<PaginationResponse<GetLocationDto>> => {
    const response = await apiClient.get<Envelope<GetLocationsResponse>>(
      "/locations",
      {
        params: {
          Search: request.search,
          "Pagination.Page": request.pagination?.page,
          "Pagination.PageSize": request.pagination?.pageSize,
        },
      }
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
    const response = await apiClient.post("/locations", request);

    return response.data;
  },
};
