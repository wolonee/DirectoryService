import { create } from "zustand";
import { useShallow } from "zustand/shallow";

export type LocationFilterState = {
  departmentIds?: string[];
  search?: string;
  status: string;
  sortBy: string;
  sortDirection: string;
  pageSize?: number;
};

type Actions = {
  setDepartmentIds: (ids: LocationFilterState["departmentIds"]) => void;
  setSearch: (input: LocationFilterState["search"]) => void;
  setStatus: (status: LocationFilterState["status"]) => void;
  setSortBy: (sortBy: LocationFilterState["sortBy"]) => void;
  setSortDirection: (sortDirection: LocationFilterState["sortDirection"]) => void;
};

type LocationsFilterStore = LocationFilterState & Actions;

const initialState = {
  departmentIds: undefined,
  search: "",
  status: "all",
  sortBy: "created_at",
  sortDirection: "desc",
  pageSize: 10
};

const useLocationsFilterStore = create<LocationsFilterStore>((set) => ({
  ...initialState,
  setDepartmentIds: (ids: LocationFilterState["departmentIds"]) =>
    set(() => ({ departmentIds: ids })),
  setSearch: (input: LocationFilterState["search"]) =>
    set(() => ({ search: input?.trim() })),
  setStatus: (status: LocationFilterState["status"]) =>
    set(() => ({ status })),
  setSortBy: (sortBy: LocationFilterState["sortBy"]) =>
    set(() => ({ sortBy })),
  setSortDirection: (sortDirection: LocationFilterState["sortDirection"]) =>
    set(() => ({ sortDirection })),
}));

export const useGetLocationFilter = () => {
  return useLocationsFilterStore(
    useShallow((state) => ({
      departmentIds: state.departmentIds,
      search: state.search,
      status: state.status,
      sortBy: state.sortBy,
      sortDirection: state.sortDirection
    })),
  );
};

export const resetLocationFilter = () => {
  const { setDepartmentIds, setSearch, setStatus, setSortBy, setSortDirection } = useLocationsFilterStore.getState();
  setDepartmentIds(undefined);
  setSearch("");
  setStatus("all");
  setSortBy("created_at");
  setSortDirection("desc");
};

export const setFilterDepartmentIds = (input: LocationFilterState["departmentIds"]) => {
  return useLocationsFilterStore.getState().setDepartmentIds(input);
};

export const setFilterSearch = (input: LocationFilterState["search"]) => {
  return useLocationsFilterStore.getState().setSearch(input);
};

export const setFilterStatus = (input: LocationFilterState["status"]) => {
  return useLocationsFilterStore.getState().setStatus(input);
};

export const setFilterSortBy = (input: LocationFilterState["sortBy"]) => {
  return useLocationsFilterStore.getState().setSortBy(input);
};

export const setFilterSortDirection = (input: LocationFilterState["sortDirection"]) => {
  return useLocationsFilterStore.getState().setSortDirection(input);
};
