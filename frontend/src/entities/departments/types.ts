import type { PaginationRequest } from "@/shared/api/types/pagination";

// ─────────────────────────── Requests ───────────────────────────

export type CreateDepartmentRequest = {
  name: string;
  identifier: string;
  parentId: string | null;
  locationIds: string[];
};

export type UpdateDepartmentLocationsRequest = {
  departmentId: string;
  locationsIds: string[];
};

export type UpdateDepartmentParentRequest = {
  departmentId: string;
  parentId: string | null;
};

export type GetDepartmentsRequest = {
  search?: string;
  locationIds?: string[];
  isActive?: boolean;
  sortBy?: "name" | "created_at";
  sortDir?: "asc" | "desc";
  pagination?: PaginationRequest;
};

export type GetDepartmentParentsByNameRequest = {
  name: string;
  pagination?: PaginationRequest;
};

// ─────────────────────────── Responses / DTO ───────────────────────────

/** POST /departments — возвращает id созданного департамента (Guid). */
export type CreateDepartmentResponse = string;

/** Общая форма узла дерева департаментов (совпадает у roots/children/ancestors). */
export type DepartmentNode = {
  id: string;
  parentId: string | null;
  name: string;
  identifier: string;
  path: string;
  depth: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
};

/** GET /departments — элемент плоского списка. */
export type GetDepartmentDto = {
  id: string;
  name: string;
  path: string;
  createdAt: string;
};

export type GetDepartmentsResponse<T> = {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
};

/** GET /departments/{id}. Внимание: на бэке поле называется `parent`, а не `parentId`. */
export type GetDepartmentByIdDto = {
  id: string;
  name: string;
  parent: string | null;
  identifier: string;
  path: string;
  depth: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
};

/** GET /departments/tree — корневой уровень. */
export type GetDepartmentRootsDto = DepartmentNode & {
  hasMoreChildren: boolean;
};

/** GET /departments/{id}/children — прямые дети узла. */
export type GetDepartmentChildrenByParentDto = DepartmentNode & {
  hasMoreChildren: boolean;
};

/** GET /departments/{id}/ancestors — предки узла (путь до корня). */
export type GetDepartmentParentsByIdDto = DepartmentNode;

/** GET /departments/tree/search — найденный департамент вместе с его предками. */
export type GetDepartmentParentsByNameDto = DepartmentNode;

export type GetDepartmentParentsByNameWithParentsDto = DepartmentNode & {
  parents: GetDepartmentParentsByNameDto[];
};

/** GET /departments/top-positions — департамент + его позиции. */
export type GetTopDepartmentsDepartmentPositionDto = {
  id: string;
  speciality: string;
  direction: string;
  isActive: boolean;
};

export type GetTopDepartmentsDepartmentDto = {
  id: string;
  name: string;
  path: string;
  depth: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  countPositions: number;
  positions: GetTopDepartmentsDepartmentPositionDto[];
};
