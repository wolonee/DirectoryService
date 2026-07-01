import { apiClient } from "@/shared/api/axios-instance";
import type { Envelope } from "@/shared/api/types/envelope";
import type { PaginationResponse } from "@/shared/api/types/pagination";
import type {
    CreateDepartmentRequest,
  GetDepartmentDto,
  GetDepartmentsRequest,
  GetDepartmentsResponse,
  UpdateDepartmentLocationsRequest,
} from "./types";
import { infiniteQueryOptions, queryOptions } from "@tanstack/react-query";

export type DepartmentsListFilter = {
  search?: string;
  isActive?: boolean;
  sortBy?: "name" | "created_at";
  sortDir?: "asc" | "desc";
  pageSize: number;
};

export const departmentsApi = {
  getDepartments: async (
    request: GetDepartmentsRequest): Promise<PaginationResponse<GetDepartmentDto>> => {
    const response = await apiClient.get<Envelope<GetDepartmentsResponse>>(
      "/departments",
      {
        params: {
          Search: request.search,
          IsActive: request.isActive,
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
      items: result.items,
      totalCount: result.totalCount,
      page,
      pageSize,
      totalPages: Math.ceil(result.totalCount / pageSize),
    };
  },

  getRoots: async (
    request: GetDepartmentsRequest): Promise<PaginationResponse<GetDepartmentDto>> => {
    const response = await apiClient.get<Envelope<GetDepartmentsResponse>>(
      "/departments",
      {
        params: {
          Search: request.search,
          IsActive: request.isActive,
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
      items: result.items,
      totalCount: result.totalCount,
      page,
      pageSize,
      totalPages: Math.ceil(result.totalCount / pageSize),
    };
  },

  createDepartment: async (request: CreateDepartmentRequest) => {
    const response = await apiClient.post("/departments", request);

    return response.data;
  },

  updateDepartmentLocations: async (request: UpdateDepartmentLocationsRequest) => {
    const { departmentId, locationsIds } = request;

    const response = await apiClient.put(
      `/departments/${departmentId}/locations`,
      { locationsIds },
    );

    return response.data;
  },

  deleteDepartment: async (departmentId: string) => {
    const response = await apiClient.delete(`/departments/${departmentId}`);

    return response.data;
  },
};

export const departmentQueryOptions = {
  baseKey: "departments",

  getAllOptions: () =>
    queryOptions({
      queryKey: ["departments", "all"],
      queryFn: () =>
        departmentsApi.getDepartments({ pagination: { page: 1, pageSize: 100 } }),
    }),

  getListInfiniteOptions: ({
    search,
    isActive,
    sortBy,
    sortDir,
    pageSize,
  }: DepartmentsListFilter) =>
    infiniteQueryOptions({
      queryKey: [
        departmentQueryOptions.baseKey,
        "list-infinite",
        { search, isActive, sortBy, sortDir, pageSize },
      ],
      queryFn: ({ pageParam }) =>
        departmentsApi.getDepartments({
          search,
          isActive,
          sortBy,
          sortDir,
          pagination: { page: pageParam, pageSize },
        }),
      initialPageParam: 1,
      getNextPageParam: (response) => {
        if (!response || response.page >= response.totalPages) return undefined;
        return response.page + 1;
      },
      select: (data): PaginationResponse<GetDepartmentDto> => {
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
    }),

  getRootsInfiniteOptions: ({
    search,
    isActive,
    sortBy,
    sortDir,
    pageSize,
  }: DepartmentsListFilter) =>
    infiniteQueryOptions({
      queryKey: [
        departmentQueryOptions.baseKey,
        "list-infinite",
        { search, isActive, sortBy, sortDir, pageSize },
      ],
      queryFn: ({ pageParam }) =>
        departmentsApi.getDepartments({
          search,
          isActive,
          sortBy,
          sortDir,
          pagination: { page: pageParam, pageSize },
        }),
      initialPageParam: 1,
      getNextPageParam: (response) => {
        if (!response || response.page >= response.totalPages) return undefined;
        return response.page + 1;
      },
      select: (data): PaginationResponse<GetDepartmentDto> => {
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
    }),
};
        
