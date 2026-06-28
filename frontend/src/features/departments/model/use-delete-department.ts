import { departmentsApi, departmentQueryOptions } from "@/entities/departments/api";
import { EnvelopeError } from "@/shared/api/types/errors";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useDeleteDepartment() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: departmentsApi.deleteDepartment,
    onSettled: () =>
      queryClient.invalidateQueries({
        queryKey: [departmentQueryOptions.baseKey],
      }),
    onError: (error) => {
      if (error instanceof EnvelopeError) {
        toast.error(error.message);
        return;
      }

      toast.error("Ошибка при удалении департамента: " + error.message);
    },
    onSuccess: () => {
      toast.success("Департамент успешно удалён");
    },
  });

  return {
    deleteDepartment: mutation.mutate,
    isPending: mutation.isPending,
  };
}
