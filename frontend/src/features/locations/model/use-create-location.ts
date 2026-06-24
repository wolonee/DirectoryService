import { locationQueryOptions, locationsApi } from "@/entities/locations/api";
import { EnvelopeError } from "@/shared/api/types/errors";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useCreateLocation(){
    const queryClient = useQueryClient();

    const mutation = useMutation({
    mutationFn: locationsApi.createLocation,
    onSettled: () => queryClient.invalidateQueries({ queryKey: [locationQueryOptions.baseKey], 
    }),
    onError: (error) => {
        if (error instanceof EnvelopeError){
          toast.error(error.message);
          return;
        }

        toast.error("Ошибка при создании локации: " + error.message);
    },
    onSuccess: () => {
        toast.success("Локация успешно создана");
    }
  })

  const envelopeError =
    mutation.error instanceof EnvelopeError ? mutation.error : undefined;

  return {
    createLocation: mutation.mutate,
    isPending: mutation.isPending,
    error: envelopeError,
    commonError: mutation.error && !envelopeError ? mutation.error : undefined,
    resetError: mutation.reset,
  }
}
