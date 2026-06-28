import { departmentsApi, departmentQueryOptions } from "@/entities/departments/api";
import { EnvelopeError } from "@/shared/api/types/errors";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useUpdateDepartmentLocations() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: departmentsApi.updateDepartmentLocations,
    onSettled: () =>
      queryClient.invalidateQueries({
        queryKey: [departmentQueryOptions.baseKey],
      }),
    onError: (error) => {
      if (error instanceof EnvelopeError) {
        toast.error(error.message);
        return;
      }

      toast.error("Ошибка при обновлении локаций: " + error.message);
    },
    onSuccess: () => {
      toast.success("Локации департамента обновлены");
    },
  });

  const envelopeError =
    mutation.error instanceof EnvelopeError ? mutation.error : undefined;

  return {
    updateDepartmentLocations: mutation.mutate,
    isPending: mutation.isPending,
    error: envelopeError,
    commonError: mutation.error && !envelopeError ? mutation.error : undefined,
    resetError: mutation.reset,
  };
}
