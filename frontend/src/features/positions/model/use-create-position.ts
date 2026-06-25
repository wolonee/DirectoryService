import { positionApi, positionQueryOptions } from "@/entities/positions/api";
import { EnvelopeError } from "@/shared/api/types/errors";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useCreatePosition(){
    const queryClient = useQueryClient();

    const mutation = useMutation({
        mutationFn: positionApi.createPosition,
        onSettled: () => queryClient.invalidateQueries({ queryKey: [positionQueryOptions.baseKey],
        }),
        onError: (error) => {
            if (error instanceof EnvelopeError){
                toast.error(error.message)
                return;
            }

            toast.error("Ошибка при создании позиции: " + error.message);
        },
        onSuccess: () => {
            toast.success("Позиция успешно создана")
        }
    })

    const envelopeError =
        mutation.error instanceof EnvelopeError ? mutation.error : undefined;

    return {
        createPosition: mutation.mutate,
        isPending: mutation.isPending,
        error: envelopeError,
        commonError: mutation.error && !envelopeError ? mutation.error : undefined,
        resetError: mutation.reset,
    }
}