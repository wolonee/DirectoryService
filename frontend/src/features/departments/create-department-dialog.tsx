"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { Plus } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/shared/components/ui/dialog";
import { Input } from "@/shared/components/ui/input";
import { Label } from "@/shared/components/ui/label";
import { Checkbox } from "@/shared/components/ui/checkbox";
import { Spinner } from "@/shared/components/ui/spinner";
import { useCreateDepartment } from "./model/use-create-department";
import { useLocationsSelect } from "./model/use-locations-select";
import { isEnvelopeError } from "@/shared/api/types/errors";

const createDepartmentSchema = z.object({
  name: z
    .string()
    .trim()
    .min(3, "Название должно содержать минимум 3 символа")
    .max(150, "Максимум 150 символов"),
  identifier: z
    .string()
    .trim()
    .min(1, "Укажите идентификатор")
    .max(150, "Максимум 150 символов"),
  locationIds: z
    .array(z.string())
    .min(1, "Выберите хотя бы одну локацию"),
});

const fieldMap = {
  Name: "name",
  Identifier: "identifier",
  LocationIds: "locationIds",
} as const;

type CreateDepartmentFormData = z.infer<typeof createDepartmentSchema>;

const initialData: CreateDepartmentFormData = {
  name: "",
  identifier: "",
  locationIds: [],
};

export function AddDepartmentDialog() {
  const [open, setOpen] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
    setError,
    watch,
    setValue,
  } = useForm<CreateDepartmentFormData>({
    resolver: zodResolver(createDepartmentSchema),
    defaultValues: initialData,
  });

  const {
    locations,
    isLoading: isLoadingLocations,
    isFetchingNextPage: isFetchingNextLocations,
    cursorRef: locationCursorRef,
  } = useLocationsSelect();

  const selectedLocationIds = watch("locationIds");

  const toggleLocation = (id: string) => {
    const current = selectedLocationIds ?? [];
    const next = current.includes(id)
      ? current.filter((l) => l !== id)
      : [...current, id];
    setValue("locationIds", next, { shouldValidate: true });
  };

  const { createDepartment, isPending, error, commonError, resetError } =
    useCreateDepartment();

  const onSubmit = (data: CreateDepartmentFormData) => {
    resetError();

    createDepartment(
      {
        name: data.name,
        identifier: data.identifier,
        parentId: null,
        locationIds: data.locationIds,
      },
      {
        onSuccess: () => {
          setOpen(false);
          reset(initialData);
        },
        onError: (error) => {
          if (!isEnvelopeError(error)) {
            return;
          }

          error.fieldErrors.forEach((fieldError) => {
            const fieldName =
              fieldMap[fieldError.invalidField as keyof typeof fieldMap];

            setError(fieldName, {
              message: fieldError.message,
            });
          });
        },
      },
    );
  };

  const handleOpenChange = (isOpen: boolean) => {
    setOpen(isOpen);

    if (!isOpen) {
      reset(initialData);
    }
  };

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogTrigger asChild>
        <Button size="lg" type="button" className="w-full sm:w-auto">
          <Plus data-icon="inline-start" />
          Добавить подразделение
        </Button>
      </DialogTrigger>

      <DialogContent className="sm:max-w-lg">
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <DialogHeader>
            <DialogTitle>Новое подразделение</DialogTitle>
            <DialogDescription>
              Укажите название и выберите локации.
            </DialogDescription>
          </DialogHeader>

          <div className="grid gap-4 py-2">
            <div className="grid gap-2">
              <Label htmlFor="department-name">Название</Label>
              <Input
                id="department-name"
                placeholder="Разработка"
                {...register("name")}
              />
              {errors.name && (
                <p className="text-xs text-destructive">{errors.name.message}</p>
              )}
            </div>

            <div className="grid gap-2">
              <Label htmlFor="department-identifier">Идентификатор</Label>
              <Input
                id="department-identifier"
                placeholder="development"
                {...register("identifier")}
              />
              {errors.identifier && (
                <p className="text-xs text-destructive">
                  {errors.identifier.message}
                </p>
              )}
            </div>

            <div className="grid gap-2">
              <Label>Локации</Label>
              <div className="max-h-40 overflow-y-auto rounded-md border border-input p-3">
                {isLoadingLocations ? (
                  <div className="flex justify-center py-2">
                    <Spinner />
                  </div>
                ) : (
                  <div className="grid gap-2">
                    {locations.map((location) => (
                      <div key={location.id} className="flex items-center gap-2">
                        <Checkbox
                          id={`location-${location.id}`}
                          checked={selectedLocationIds.includes(location.id)}
                          onCheckedChange={() => toggleLocation(location.id)}
                        />
                        <Label
                          htmlFor={`location-${location.id}`}
                          className="cursor-pointer font-normal"
                        >
                          {location.name}{" "}
                          <span className="text-muted-foreground">
                            · {location.city}, {location.country}
                          </span>
                        </Label>
                      </div>
                    ))}
                    <div
                      ref={locationCursorRef}
                      className="flex justify-center py-1"
                    >
                      {isFetchingNextLocations && <Spinner />}
                    </div>
                  </div>
                )}
              </div>
              {errors.locationIds && (
                <p className="text-xs text-destructive">
                  {errors.locationIds.message}
                </p>
              )}
            </div>
          </div>

          {error && (
            <p className="text-sm text-destructive">{error.message}</p>
          )}

          {commonError && (
            <p className="text-sm text-destructive">
              Не удалось создать департамент. Попробуйте позже.
            </p>
          )}

          <DialogFooter>
            <DialogClose asChild>
              <Button type="button" variant="outline">
                Отмена
              </Button>
            </DialogClose>
            <Button type="submit" disabled={isPending}>
              {isPending ? "Добавляем..." : "Добавить"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
