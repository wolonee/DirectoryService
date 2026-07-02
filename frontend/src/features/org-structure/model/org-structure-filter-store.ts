import { create } from "zustand";
import { useShallow } from "zustand/react/shallow";

export type OrgStructureFilterState = {
    selectedId: string | null;
    expandedIds: string[];
};

type Actions = {
    setSelectedId: (id: string | null) => void;
    setExpandedIds: (ids: string[]) => void;
};

type OrgStructureFilterStore = OrgStructureFilterState & Actions;

const initialState = {
    selectedId: null,
    expandedIds: [],
}

const useOrgStructureFilterStore = create<OrgStructureFilterStore>((set) => ({
    ...initialState,
    setSelectedId: (id: string | null) => set(() => ({ selectedId: id })),
    setExpandedIds: (ids: string[]) => set(() => ({ expandedIds: ids })),
}));

export const useGetOrgStructureFilter = () => {
    return useOrgStructureFilterStore(
        useShallow((state) => ({
            selectedId: state.selectedId,
            expandedIds: state.expandedIds,
        })),
    );
};

export const setOrgStructureFilterSelectedId = (id: string | null) => {
    return useOrgStructureFilterStore.getState().setSelectedId(id);
};

export const setOrgStructureFilterExpandedIds = (ids: string[]) => {
    return useOrgStructureFilterStore.getState().setExpandedIds(ids);
};