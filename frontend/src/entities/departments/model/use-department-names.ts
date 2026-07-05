import { useQueries } from "@tanstack/react-query";
import { departmentQueryOptions } from "../api";

/**
 * Догружает названия департаментов по их id (через getById) и отдаёт карту id -> name.
 * Нужен, когда выбранный департамент отсутствует в загруженном списке select-а
 * (например, восстановлен из URL) — иначе в бейдже показался бы сырой id.
 */
export function useDepartmentNames(ids: string[]): Map<string, string> {
  const results = useQueries({
    queries: ids.map((id) => departmentQueryOptions.getByIdOptions(id)),
  });

  const nameById = new Map<string, string>();
  results.forEach((result, index) => {
    const name = result.data?.name;
    if (name) {
      nameById.set(ids[index], name);
    }
  });

  return nameById;
}
