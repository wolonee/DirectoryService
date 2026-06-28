import { create } from "zustand";
import { useShallow } from "zustand/shallow";

export type DepartmentFilterState = {
  search?: string;
  isActive?: boolean;
  sortBy: "name" | "created_at";
  sortDir: "asc" | "desc";
};

type Actions = {
  setSearch: (input: DepartmentFilterState["search"]) => void;
  setIsActive: (isActive: DepartmentFilterState["isActive"]) => void;
  setSortBy: (sortBy: DepartmentFilterState["sortBy"]) => void;
  setSortDir: (sortDir: DepartmentFilterState["sortDir"]) => void;
};

type DepartmentsFilterStore = DepartmentFilterState & Actions;

const initialState = {
  search: "",
  isActive: undefined,
  sortBy: "created_at",
  sortDir: "desc",
} satisfies DepartmentFilterState;

const useDepartmentsFilterStore = create<DepartmentsFilterStore>((set) => ({
  ...initialState,
  setSearch: (input: DepartmentFilterState["search"]) =>
    set(() => ({ search: input?.trim() })),
  setIsActive: (isActive: DepartmentFilterState["isActive"]) =>
    set(() => ({ isActive })),
  setSortBy: (sortBy: DepartmentFilterState["sortBy"]) =>
    set(() => ({ sortBy })),
  setSortDir: (sortDir: DepartmentFilterState["sortDir"]) =>
    set(() => ({ sortDir })),
}));

export const useGetDepartmentFilter = () => {
  return useDepartmentsFilterStore(
    useShallow((state) => ({
      search: state.search,
      isActive: state.isActive,
      sortBy: state.sortBy,
      sortDir: state.sortDir,
    })),
  );
};

export const setDepartmentFilterSearch = (
  input: DepartmentFilterState["search"],
) => {
  return useDepartmentsFilterStore.getState().setSearch(input);
};

export const setDepartmentFilterIsActive = (
  input: DepartmentFilterState["isActive"],
) => {
  return useDepartmentsFilterStore.getState().setIsActive(input);
};

export const setDepartmentFilterSortBy = (
  input: DepartmentFilterState["sortBy"],
) => {
  return useDepartmentsFilterStore.getState().setSortBy(input);
};

export const setDepartmentFilterSortDir = (
  input: DepartmentFilterState["sortDir"],
) => {
  return useDepartmentsFilterStore.getState().setSortDir(input);
};
