import { apiClient } from "@/shared/api/axios-instance";
import type {
  CreateLocationRequest,
  GetLocationsRequest,
  GetLocationsResponse,
} from "./types";
import { Envelope } from "@/shared/api/envelope";

export const locationsApi = {
  getLocations: async (
    request: GetLocationsRequest
  ): Promise<GetLocationsResponse> => {
    const response = await apiClient.get<Envelope<GetLocationsResponse>>("/locations", {
      params: {
        Search: request.search,
        "Pagination.Page": request.page,
        "Pagination.PageSize": request.pageSize,
      },
    });

    return response.data.result ?? { locations: [], totalCount: 0 };
  },

  createLocation: async (request: CreateLocationRequest) => {
    const response = await apiClient.post("/locations", request);

    return response.data;
  },
};
