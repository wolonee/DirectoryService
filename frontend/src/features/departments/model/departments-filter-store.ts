import { create } from "zustand";
import { useShallow } from "zustand/shallow";

export type DepartmentFilterState = {
  search?: string;
  status: string;
  sortBy: "name" | "created_at";
  sortDir: "asc" | "desc";
};

type Actions = {
  setSearch: (input: DepartmentFilterState["search"]) => void;
  setStatus: (status: DepartmentFilterState["status"]) => void;
  setSortBy: (sortBy: DepartmentFilterState["sortBy"]) => void;
  setSortDir: (sortDir: DepartmentFilterState["sortDir"]) => void;
};

type DepartmentsFilterStore = DepartmentFilterState & Actions;

const initialState = {
  search: "",
  status: "all",
  sortBy: "created_at",
  sortDir: "desc",
} satisfies DepartmentFilterState;

const useDepartmentsFilterStore = create<DepartmentsFilterStore>((set) => ({
  ...initialState,
  setSearch: (input: DepartmentFilterState["search"]) =>
    set(() => ({ search: input?.trim() })),
  setStatus: (status: DepartmentFilterState["status"]) =>
    set(() => ({ status })),
  setSortBy: (sortBy: DepartmentFilterState["sortBy"]) =>
    set(() => ({ sortBy })),
  setSortDir: (sortDir: DepartmentFilterState["sortDir"]) =>
    set(() => ({ sortDir })),
}));

export const useGetDepartmentFilter = () => {
  return useDepartmentsFilterStore(
    useShallow((state) => ({
      search: state.search,
      status: state.status,
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

export const setDepartmentFilterStatus = (
  input: DepartmentFilterState["status"],
) => {
  return useDepartmentsFilterStore.getState().setStatus(input);
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
