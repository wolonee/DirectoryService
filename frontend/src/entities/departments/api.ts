import { apiClient } from "@/shared/api/axios-instance";
import type { Envelope } from "@/shared/api/types/envelope";
import type { PaginationResponse } from "@/shared/api/types/pagination";
import type {
    CreateDepartmentRequest,
  GetDepartmentDto,
  GetDepartmentsRequest,
  GetDepartmentsResponse,
} from "./types";

export const departmentsApi = {
  getDepartments: async (
    request: GetDepartmentsRequest): Promise<PaginationResponse<GetDepartmentDto>> => {
    const response = await apiClient.get<Envelope<GetDepartmentsResponse>>(
      "/departments",
      {
        params: {
          Search: request.search,
          SortBy: request.sortBy,
          SortDir: request.sortDir,
          "Pagination.Page": request.pagination?.page,
          "Pagination.PageSize": request.pagination?.pageSize,
        },
      }
    );

    const page = request.pagination?.page ?? 1;
    const pageSize = request.pagination?.pageSize ?? 20;
    const result = response.data.result;

    if (!result) {
      return {
        items: [],
        totalCount: 0,
        page,
        pageSize,
        totalPages: 0,
      };
    }

    return {
      items: result.departments,
      totalCount: result.totalCount,
      page,
      pageSize,
      totalPages: Math.ceil(result.totalCount / pageSize),
    };
  },

  createDepartment: async (request: CreateDepartmentRequest) => {
    const response = await apiClient.post("/departments", request);

    return response.data;
  }
};
        
