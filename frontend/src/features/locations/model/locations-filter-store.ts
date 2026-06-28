import { create } from "zustand";
import { useShallow } from "zustand/shallow";

export type LocationFilterState = {
  search?: string;
  isActive?: boolean; 
  sortBy: string;
  sortDirection: string;
  pageSize?: number;
};

type Actions = {
  setSearch: (input: LocationFilterState["search"]) => void;
  setIsActive: (isActive: LocationFilterState["isActive"]) => void;
  setSortBy: (sortBy: LocationFilterState["sortBy"]) => void;
  setSortDirection: (sortDirection: LocationFilterState["sortDirection"]) => void;
};

type LocationsFilterStore = LocationFilterState & Actions;

const initialState = {
  search: "",
  isActive: undefined,
  sortBy: "created_at",
  sortDirection: "desc",
  pageSize: 10
};

const useLocationsFilterStore = create<LocationsFilterStore>((set) => ({
  ...initialState,
  setSearch: (input: LocationFilterState["search"]) =>
    set(() => ({ search: input?.trim() })),
  setIsActive: (isActive: LocationFilterState["isActive"]) =>
    set(() => ({ isActive })),
  setSortBy: (sortBy: LocationFilterState["sortBy"]) =>
    set(() => ({ sortBy })),
  setSortDirection: (sortDirection: LocationFilterState["sortDirection"]) =>
    set(() => ({ sortDirection })),
}));

export const useGetLocationFilter = () => {
  return useLocationsFilterStore(
    useShallow((state) => ({
      search: state.search,
      isActive: state.isActive,
      sortBy: state.sortBy,
      sortDirection: state.sortDirection
    })),
  );
};

export const setFilterSearch = (input: LocationFilterState["search"]) => {
  return useLocationsFilterStore.getState().setSearch(input);
};

export const setFilterIsActive = (input: LocationFilterState["isActive"]) => {
  return useLocationsFilterStore.getState().setIsActive(input);
};

export const setFilterSortBy = (input: LocationFilterState["sortBy"]) => {
  return useLocationsFilterStore.getState().setSortBy(input);
};

export const setFilterSortDirection = (input: LocationFilterState["sortDirection"]) => {
  return useLocationsFilterStore.getState().setSortDirection(input);
};

