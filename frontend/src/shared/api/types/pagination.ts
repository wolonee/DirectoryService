export type PaginationResponse<T> = {
    items: T[];
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
}

export type PaginationRequest = {
  page: number;
  pageSize?: number;
};