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
};

export type CreateLocationRequest = {
  nothing: string;
};

export type GetLocationsRequest = {
  search?: string;
  page: number;
  pageSize: number;
};
