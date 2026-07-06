import { locationQueryOptions, locationsApi } from "@/entities/locations/api";
import { EnvelopeError } from "@/shared/api/types/errors";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useRestoreLocation() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: (id: string) => locationsApi.restoreLocation(id),
    onSettled: () =>
      queryClient.invalidateQueries({
        queryKey: [locationQueryOptions.baseKey],
      }),
    onError: (error) => {
      if (error instanceof EnvelopeError) {
        toast.error(error.message);
        return;
      }

      toast.error("Ошибка при восстановлении локации: " + error.message);
    },
    onSuccess: () => {
      toast.success("Локация успешно восстановлена");
    },
  });

  return {
    restoreLocation: mutation.mutate,
    isPending: mutation.isPending,
  };
}
