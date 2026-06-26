import { locationQueryOptions, locationsApi } from "@/entities/locations/api";
import { EnvelopeError } from "@/shared/api/types/errors";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useUpdateLocation(){
    const queryClient = useQueryClient();

    const mutation = useMutation({
    mutationFn: locationsApi.updateLocation,
    onSettled: () => queryClient.invalidateQueries({ queryKey: [locationQueryOptions.baseKey], 
    }),
    onError: (error) => {
        if (error instanceof EnvelopeError){
          toast.error(error.message);
          return;
        }

        toast.error("Ошибка при обновлении локации: " + error.message);
    },
    onSuccess: () => {
        toast.success("Локация успешно обновлена");
    }
  })

  const envelopeError =
    mutation.error instanceof EnvelopeError ? mutation.error : undefined;

  return {
    updateLocation: mutation.mutate,
    isPending: mutation.isPending,
    error: envelopeError,
    commonError: mutation.error && !envelopeError ? mutation.error : undefined,
    resetError: mutation.reset,
  }
}
