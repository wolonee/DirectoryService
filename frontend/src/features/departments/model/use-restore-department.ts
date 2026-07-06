import { departmentsApi, departmentQueryOptions } from "@/entities/departments/api";
import { EnvelopeError } from "@/shared/api/types/errors";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useRestoreDepartment() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: departmentsApi.restoreDepartment,
    onSettled: () =>
      queryClient.invalidateQueries({
        queryKey: [departmentQueryOptions.baseKey],
      }),
    onError: (error) => {
      if (error instanceof EnvelopeError) {
        toast.error(error.message);
        return;
      }

      toast.error("Ошибка при восстановлении подразделения: " + error.message);
    },
    onSuccess: () => {
      toast.success("Подразделение успешно восстановлено");
    },
  });

  return {
    restoreDepartment: mutation.mutate,
    isPending: mutation.isPending,
  };
}
