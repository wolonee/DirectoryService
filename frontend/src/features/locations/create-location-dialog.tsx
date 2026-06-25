"use client";

import { Button } from "@/shared/components/ui/button";
import { Dialog, DialogClose, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from "@/shared/components/ui/dialog";
import { Input } from "@/shared/components/ui/input";
import { Label } from "@/shared/components/ui/label";
import { Plus } from "lucide-react";
import { useCreateLocation } from "./model/use-create-location";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { isEnvelopeError } from "@/shared/api/types/errors";
import { useState } from "react";

const createLocationSchema = z.object({
  name: z
    .string()
    .trim()
    .min(3, "Название должно содержать минимум 3 символа")
    .max(120, "Название должно содержать максимум 120 символов"),
  country: z.string().trim().min(1, "Укажите страну"),
  city: z.string().trim().min(1, "Укажите город"),
  street: z.string().trim().min(1, "Укажите улицу и дом"),
  timezone: z
    .string()
    .trim()
    .min(1, "Укажите часовой пояс")
    .regex(
      /^[A-Za-z_]+(?:\/[A-Za-z0-9_+-]+)+$/,
      "Используйте формат Europe/Moscow"
    ),
});

type CreateLocationFormData = z.infer<typeof createLocationSchema>;

const initialData: CreateLocationFormData = {
  name: "",
  country: "",
  city: "",
  street: "",
  timezone: "",
};

const fieldMap = {
  Name: "name",
  Timezone: "timezone",
  "Address.Country": "country",
  "Address.City": "city",
  "Address.Street": "street",
} as const;

export function AddLocationDialog() {
  const [open, setOpen] = useState(false);

  const { register, handleSubmit, formState: { errors }, reset, setError } = useForm<CreateLocationFormData>({
    resolver: zodResolver(createLocationSchema),
    defaultValues: initialData,
  });

  const { createLocation, isPending, error, commonError, resetError } =
    useCreateLocation();

  const onSubmit = (data: CreateLocationFormData) => {
    resetError();

    createLocation(
      {
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
          reset(initialData);
        },
        onError: (error) => {
          if (!(isEnvelopeError(error))) {
            return;
          }

          error.fieldErrors.forEach((fieldError) => {
            const fieldName = fieldMap[fieldError.invalidField as keyof typeof fieldMap];
            
            setError(fieldName, {
              message: fieldError.message,
            });
          });
        },
      }
    );
  }

  const handleOpenChange = (isOpen: boolean) => {
    setOpen(isOpen);

    if (!isOpen) {
      resetError();
    }
  };
  
  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogTrigger asChild>
        <Button size="lg" type="button" className="w-full sm:w-auto">
          <Plus data-icon="inline-start" />
          Добавить локацию
        </Button>
      </DialogTrigger>

      <DialogContent className="sm:max-w-lg">
        <form onSubmit={handleSubmit((data) => onSubmit(data))} className="space-y-4">
          <DialogHeader>
            <DialogTitle>Новая локация</DialogTitle>
            <DialogDescription>
              Заполните информацию об офисе или рабочей площадке.
            </DialogDescription>
          </DialogHeader>

          <div className="grid gap-4 py-2">
            <div className="grid gap-2">
              <Label htmlFor="location-name">Название</Label>
              <Input
                id="location-name"
                placeholder="Главный офис"
                {...register("name")}
              />
              { errors.name && (
                <p className="text-xs text-destructive">{errors.name.message}</p>
              )}
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="grid gap-2">
                <Label htmlFor="location-country">Страна</Label>
                <Input
                  id="location-country"
                  placeholder="Россия"
                  {...register("country")}
                />
                { errors.country && (
                  <p className="text-xs text-destructive">{errors.country.message}</p>
                )}
              </div>

              <div className="grid gap-2">
                <Label htmlFor="location-city">Город</Label>
                <Input
                  id="location-city"
                  placeholder="Москва"
                  {...register("city")}
                />
                { errors.city && (
                  <p className="text-xs text-destructive">{errors.city.message}</p>
                )}
              </div>
            </div>

            <div className="grid gap-2">
              <Label htmlFor="location-street">Улица и дом</Label>
              <Input
                id="location-street"
                placeholder="ул. Ленина, 1"
                {...register("street")}
              />
              { errors.street && (
                <p className="text-xs text-destructive">{errors.street.message}</p>
              )}
            </div>

            <div className="grid gap-2">
              <Label htmlFor="location-timezone">Часовой пояс</Label>
              <Input
                id="location-timezone"
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
