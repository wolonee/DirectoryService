import { apiClient } from "@/shared/api/axios-instance";
import type { Envelope } from "@/shared/api/types/envelope";
import type { PaginationRequest, PaginationResponse } from "@/shared/api/types/pagination";
import type {
  CreateDepartmentRequest,
  GetDepartmentByIdDto,
  GetDepartmentChildrenByParentDto,
  GetDepartmentDto,
  GetDepartmentPositionsDto,
  GetDepartmentRootsDto,
  GetDepartmentsRequest,
  GetDepartmentsResponse,
  UpdateDepartmentLocationsRequest,
  UpdateDepartmentParentRequest,
} from "./types";
import { infiniteQueryOptions, queryOptions } from "@tanstack/react-query";

export type DepartmentsListFilter = {
  search?: string;
  status?: string;
  sortBy?: "name" | "created_at";
  sortDir?: "asc" | "desc";
  pageSize: number;
};

export const departmentsApi = {
  getDepartments: async (
    request: GetDepartmentsRequest): Promise<PaginationResponse<GetDepartmentDto>> => {
    const response = await apiClient.get<Envelope<GetDepartmentsResponse<GetDepartmentDto>>
    >("/departments", {
      params: {
        Search: request.search,
        Status: request.status,
        SortBy: request.sortBy,
        SortDir: request.sortDir,
        "Pagination.Page": request.pagination?.page,
        "Pagination.PageSize": request.pagination?.pageSize,
      },
    });

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
      items: result.items,
      totalCount: result.totalCount,
      page: result.page,
      pageSize: result.pageSize,
      totalPages: result.totalPages,
    };
  },

  getRoots: async () : Promise<PaginationResponse<GetDepartmentRootsDto>> => {
    const response = await apiClient.get<Envelope<GetDepartmentsResponse<GetDepartmentRootsDto>>>(
      "/departments/tree"
    );

    const result = response.data.result;

    if (!result) {
      return {
        items: [],
        totalCount: 0,
        page: 1,
        pageSize: 20,
        totalPages: 0,
      };
    }

    return {
      items: result.items,
      totalCount: result.totalCount,
      page: result.page,
      pageSize: result.pageSize,
      totalPages: result.totalPages,
    };
  },

  getById: async (id: string): Promise<GetDepartmentByIdDto | null> => {
    const response = await apiClient.get<Envelope<GetDepartmentByIdDto>>(
      `/departments/${id}`
    );

    const result = response.data.result;

    if (!result) {
      return null;
    }

    return result;
  },

  getChildrenById: async (id: string): Promise<PaginationResponse<GetDepartmentChildrenByParentDto>> => {
    const response = await apiClient.get<Envelope<GetDepartmentsResponse<GetDepartmentChildrenByParentDto>>>(
      `/departments/${id}/children`
    );

    const result = response.data.result;

    if (!result) {
      return {
        items: [],
        totalCount: 0,
        page: 1,
        pageSize: 20,
        totalPages: 0,
      };
    }

    return {
      items: result.items,
      totalCount: result.totalCount,
      page: result.page,
      pageSize: result.pageSize,
      totalPages: result.totalPages,
    };
  },

  createDepartment: async (request: CreateDepartmentRequest) => {
    const response = await apiClient.post("/departments", request);

    return response.data;
  },

  updateDepartmentParent: async (request: UpdateDepartmentParentRequest) => {
    const response = await apiClient.put(
      `/departments/${request.departmentId}/parent`,
      { parentId: request.parentId }
    );

    return response.data;
  },

  updateDepartmentLocations: async (
    request: UpdateDepartmentLocationsRequest,
  ) => {
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

  restoreDepartment: async (departmentId: string) => {
    const response = await apiClient.post(`/departments/${departmentId}/restore`);

    return response.data;
  },

  getPositionsByDepartment: async (departmentId: string, pagination?: PaginationRequest): Promise<PaginationResponse<GetDepartmentPositionsDto>> => {
    const response = await apiClient.get<Envelope<GetDepartmentsResponse<GetDepartmentPositionsDto>>>(
      `/departments/${departmentId}/positions`,
      {
        params: {
          "Pagination.Page": pagination?.page,
          "Pagination.PageSize": pagination?.pageSize,
        },
      },
    );

    const result = response.data.result;

    if (!result) {
      return {
        items: [],
        totalCount: 0,
        page: pagination?.page ?? 1,
        pageSize: pagination?.pageSize ?? 20,
        totalPages: 0,
      };
    }

    return {
      items: result.items,
      totalCount: result.totalCount,
      page: result.page,
      pageSize: result.pageSize,
      totalPages: result.totalPages,
    };
  }
};

export const departmentQueryOptions = {
  baseKey: "departments",

  getAllOptions: () =>
    queryOptions({
      queryKey: ["departments", "all"],
      queryFn: () =>
        departmentsApi.getDepartments({
          pagination: { page: 1, pageSize: 100 },
        }),
    }),

  getRootsOptions: () =>
    queryOptions({
      queryKey: ["departments", "roots"],
      queryFn: () => departmentsApi.getRoots(),
    }),

  getByIdOptions: (id: string) =>
    queryOptions({
      queryKey: ["departments", "by-id", id],
      queryFn: () => departmentsApi.getById(id),
    }),

  getChildrenOptions: (parentId: string) =>
    queryOptions({
      queryKey: ["departments", "children", parentId],
      queryFn: () => departmentsApi.getChildrenById(parentId),
    }),

  getPositionsByDepartmentIdInfiniteOptions: (departmentId: string, pageSize: number) =>
    infiniteQueryOptions({
      queryKey: ["departments", "positions", departmentId, { pageSize }],
      queryFn: ({ pageParam }) =>
        departmentsApi.getPositionsByDepartment(departmentId, {
          page: pageParam,
          pageSize,
        }),
      initialPageParam: 1,
      getNextPageParam: (response) => {
        if (!response || response.page >= response.totalPages) return undefined;
        return response.page + 1;
      },
      select: (data): PaginationResponse<GetDepartmentPositionsDto> => {
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

  getListInfiniteOptions: ({
    search,
    status,
    sortBy,
    sortDir,
    pageSize,
  }: DepartmentsListFilter) =>
    infiniteQueryOptions({
      queryKey: [
        departmentQueryOptions.baseKey,
        "list-infinite",
        { search, status, sortBy, sortDir, pageSize },
      ],
      queryFn: ({ pageParam }) =>
        departmentsApi.getDepartments({
          search,
          status,
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
