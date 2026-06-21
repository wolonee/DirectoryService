import { PaginationRequest } from "@/shared/api/types/pagination";

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
  locations: GetLocationDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
};

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

export type GetLocationsRequest = {
  search?: string;
  pagination?: PaginationRequest;
};

