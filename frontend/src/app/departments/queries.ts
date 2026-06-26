import { departmentsApi } from "@/entities/departments/api";
import { keepPreviousData } from "@tanstack/react-query";

export function getDepartmentsQueryOptions(page: number, pageSize: number) {
    return {
        queryKey: ["departments", { page }],
        queryFn: () => departmentsApi.getDepartments({
            pagination: {
                page,
                pageSize,
            }
        }),
        placeholderData: keepPreviousData,
    }
};