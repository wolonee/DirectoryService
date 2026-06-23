import { locationQueryOptions, locationsApi } from "@/entities/locations/api";
import { QueryClient, useMutation } from "@tanstack/react-query";
import { toast } from "sonner";

export function useCreateLocation(){
    const queryClient = new QueryClient();

    const mutation = useMutation({
    mutationFn: locationsApi.createLocation,
    onSettled: () => queryClient.invalidateQueries({ queryKey: [locationQueryOptions.baseKey], 
    }),
    onError: (error) => {
        toast.error("Ошибка при создании локации: " + error.message);
    },
    onSuccess: () => {
        toast.success("Локация успешно создана");
    }
  })

  return {
    createLocation: mutation.mutate,
    isPending: mutation.isPending,
    error: mutation.error,
  }
}