import { departmentQueryOptions, departmentsApi } from "@/entities/departments/api";
import { EnvelopeError } from "@/shared/api/types/errors";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useUpdateDepartmentParent() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: (data: { departmentId: string; newParentId: string }) =>
      departmentsApi.updateDepartmentParent({ departmentId: data.departmentId, parentId: data.newParentId }),
    onSettled: () =>
      queryClient.invalidateQueries({queryKey: [departmentQueryOptions.baseKey],}),
    onError: (error) => {
      if (error instanceof EnvelopeError) {
        toast.error(error.message);
        return;
      }
      toast.error("Ошибка при перемещении департамента: " + error.message);
    },
    onSuccess: () => {
      toast.success("Департамент успешно перемещен");
    },
  });

  const envelopeError =
    mutation.error instanceof EnvelopeError ? mutation.error : undefined;

  return {
    updateDepartmentParent: mutation.mutate,
    isPending: mutation.isPending,
    error: envelopeError,
    commonError: mutation.error && !envelopeError ? mutation.error : undefined,
    resetError: mutation.reset,
  };
}