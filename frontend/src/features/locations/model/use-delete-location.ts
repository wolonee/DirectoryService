import { locationQueryOptions, locationsApi } from "@/entities/locations/api";
import { EnvelopeError } from "@/shared/api/types/errors";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useDeleteLocation() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: locationsApi.deleteLocation,
    onSettled: () =>
      queryClient.invalidateQueries({
        queryKey: [locationQueryOptions.baseKey],
      }),
    onError: (error) => {
      if (error instanceof EnvelopeError) {
        toast.error(error.message);
        return;
      }

      toast.error("Ошибка при удалении локации: " + error.message);
    },
    onSuccess: () => {
      toast.success("Локация успешно удалена");
    },
  });

  return {
    deleteLocation: mutation.mutate,
    isPending: mutation.isPending,
  };
}
