"use client";

import { GetLocationDto } from "@/entities/locations/types";
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
import { zodResolver } from "@hookform/resolvers/zod";
import { Pencil } from "lucide-react";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { useUpdateLocation } from "./model/use-update-location";
import { isEnvelopeError } from "@/shared/api/types/errors";
import {
  locationSchema,
  LocationFormData,
} from "@/entities/locations/model/location-schema";

const fieldMap = {
  Name: "name",
  Timezone: "timezone",
  "Address.Country": "country",
  "Address.City": "city",
  "Address.Street": "street",
} as const;

type Props = {
  location: GetLocationDto;
};

export function UpdateLocationDialog({ location }: Props) {
  const [open, setOpen] = useState(false);

  const initialData: LocationFormData = {
    name: location.name,
    country: location.country,
    city: location.city,
    street: location.street,
    timezone: location.timezone,
  };

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
    setError,
  } = useForm<LocationFormData>({
    resolver: zodResolver(locationSchema),
    defaultValues: initialData,
  });

  const { updateLocation, isPending, error, commonError, resetError } =
    useUpdateLocation();

  const onSubmit = (data: LocationFormData) => {
    updateLocation(
      {
        locationId: location.id,
        address: {
          city: data.city,
          country: data.country,
          street: data.street,
        },
        name: data.name,
        timezone: data.timezone,
      },
      {
        onSuccess: () => {
          setOpen(false);
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
      resetError();
      reset(initialData)
    }
  };

  return (
    <Dialog open={open} onOpenChange={(bool) => handleOpenChange(bool)}>
      <DialogTrigger asChild>
        <Button
          variant="ghost"
          size="icon"
          type="button"
          aria-label={`Редактировать ${location.name}`}>
          <Pencil />
        </Button>
      </DialogTrigger>

      <DialogContent className="sm:max-w-lg">
        <form
          onSubmit={handleSubmit((data) => onSubmit(data))}
          className="space-y-4">
          <DialogHeader>
            <DialogTitle>Редактировать локацию</DialogTitle>
            <DialogDescription>
              Измените информацию об офисе или рабочей площадке.
            </DialogDescription>
          </DialogHeader>

          <div className="grid gap-4 py-2">
            <div className="grid gap-2">
              <Label htmlFor="update-location-name">Название</Label>
              <Input
                id="update-location-name"
                placeholder="Главный офис"
                {...register("name")}
                />
              { errors.name && (
                <p className="text-xs text-destructive">{errors.name.message}</p>
              )}
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="grid gap-2">
                <Label htmlFor="update-location-country">Страна</Label>
                <Input
                  id="update-location-country"
                  placeholder="Россия"
                  {...register("country")}
                />
              { errors.country && (
                <p className="text-xs text-destructive">{errors.country.message}</p>
              )}
              </div>

              <div className="grid gap-2">
                <Label htmlFor="update-location-city">Город</Label>
                <Input
                  id="update-location-city"
                  placeholder="Москва"
                  {...register("city")}
                />
              { errors.city && (
                <p className="text-xs text-destructive">{errors.city.message}</p>
              )}
              </div>
            </div>

            <div className="grid gap-2">
              <Label htmlFor="update-location-street">Улица и дом</Label>
              <Input
                id="update-location-street"
                placeholder="ул. Ленина, 1"
                {...register("street")}
              />
              { errors.street && (
                <p className="text-xs text-destructive">{errors.street.message}</p>
              )}
            </div>

            <div className="grid gap-2">
              <Label htmlFor="update-location-timezone">Часовой пояс</Label>
              <Input
                id="update-location-timezone"
                placeholder="Europe/Moscow"
                {...register("timezone")}
              />
              { errors.timezone && (
                <p className="text-xs text-destructive">{errors.timezone.message}</p>
              )}
            </div>
          </div>

          {error && (
            <p className="text-sm text-destructive">
              {error.message}
            </p>
          )}

          {commonError && (
            <p className="text-sm text-destructive">
              Не удалось создать локацию. Попробуйте позже.
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
