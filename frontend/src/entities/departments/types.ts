import type { PaginationRequest } from "@/shared/api/types/pagination";

export type CreateDepartmentRequest = {
  name: string;
  identifier: string;
  parentId: string | null;
  locationIds: string[];
};

export type CreateDepartmentResponse = string;

export type UpdateDepartmentLocationsRequest = {
  departmentId: string;
  locationsIds: string[];
};

export type GetDepartmentsRequest = {
  search?: string;
  isActive?: boolean;
  sortBy?: "name" | "created_at";
  sortDir?: "asc" | "desc";
  pagination?: PaginationRequest;
};

export type GetDepartmentDto = {
  id: string;
  name: string;
  path: string;
  createdAt: string;
};

export type GetDepartmentsResponse = {
  items: GetDepartmentDto[];
  totalCount: number;
};
