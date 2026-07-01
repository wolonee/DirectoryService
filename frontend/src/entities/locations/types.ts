import { PaginationRequest } from "@/shared/api/types/pagination";

// ─────────────────────────── Requests ───────────────────────────

export type CreateLocationAddressRequest = {
  country: string;
  city: string;
  street: string;
};

export type CreateLocationRequest = {
  address: CreateLocationAddressRequest;
  name: string;
  timezone: string;
};

export type UpdateLocationRequest = {
  locationId: string;
  address: CreateLocationAddressRequest;
  name: string;
  timezone: string;
};

export type GetLocationsRequest = {
  departmentIds?: string[];
  minDepartmentCount?: number;
  search?: string;
  isActive?: boolean;
  sortBy?: string;
  sortDirection?: string;
  pagination?: PaginationRequest;
};

// ─────────────────────────── Responses / DTO ───────────────────────────

/** GET /locations — элемент списка. */
export type GetLocationDto = {
  id: string;
  name: string;
  country: string;
  city: string;
  street: string;
  timezone: string;
  createdAt: string;
  countDepartments: number;
};

export type GetLocationsResponse = {
  items: GetLocationDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
};

/** GET /locations/{id}. */
export type GetLocationByIdResponse = {
  id: string;
  name: string;
  country: string;
  city: string;
  street: string;
  timezone: string;
};

export const LocationSortByOptions = {
  name: "name",
  created_at: "created_at",
  country: "country",
};
