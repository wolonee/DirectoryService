import { departmentsApi, departmentQueryOptions } from "@/entities/departments/api";
import { EnvelopeError } from "@/shared/api/types/errors";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useCreateDepartment() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: departmentsApi.createDepartment,
    onSettled: () =>
      queryClient.invalidateQueries({
        queryKey: [departmentQueryOptions.baseKey],
      }),
    onError: (error) => {
      if (error instanceof EnvelopeError) {
        toast.error(error.message);
        return;
      }

      toast.error("Ошибка при создании департамента: " + error.message);
    },
    onSuccess: () => {
      toast.success("Департамент успешно создан");
    },
  });

  const envelopeError =
    mutation.error instanceof EnvelopeError ? mutation.error : undefined;

  return {
    createDepartment: mutation.mutate,
    isPending: mutation.isPending,
    error: envelopeError,
    commonError: mutation.error && !envelopeError ? mutation.error : undefined,
    resetError: mutation.reset,
  };
}
