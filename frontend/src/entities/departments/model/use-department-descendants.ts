import { departmentQueryOptions } from "@/entities/departments/api";
import { useQuery } from "@tanstack/react-query";

/**
 * Все потомки узла (весь поддерев) по id родителя.
 * Запрос уходит только когда enabled === true (например, при открытом диалоге).
 * Возвращает и сами узлы, и готовый список их id — удобно для исключения из выбора.
 */
export function useDepartmentDescendants(parentId: string, enabled: boolean) {
  const { data, isLoading, isError, refetch } = useQuery({
    ...departmentQueryOptions.getDescendantsOptions(parentId),
    enabled: enabled && !!parentId,
  });

  const descendants = data ?? [];

  return {
    descendants,
    descendantIds: descendants.map((d) => d.id),
    isLoading,
    isError,
    refetch,
  };
}
