import { PaginationRequest } from "@/shared/api/types/pagination";

export type GetPositionDto = {
  id: string;
  speciality: string;
  direction: string;
  createdAt: string;
};

export type PositionNameRequest = {
  speciality: string;
  direction: string;
};

export type CreatePositionRequest = {
  positionName: PositionNameRequest;
  description?: string;
  departmentIds: string[];
};

export type RenamePositionRequest = {
  positionName: PositionNameRequest;
};

export type GetPositionsRequest = {
  search?: string;
  sortBy?: string;
  sortDir?: string;
  pagination?: PaginationRequest;
};

export type GetPositionsResponse = {
  positions: GetPositionDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
};
