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

const createPositionSchema = z.object({
  speciality: z
    .string()
    .trim()
    .min(1, "Укажите специальность")
    .max(100, "Максимум 100 символов"),
  direction: z
    .string()
    .trim()
    .min(1, "Укажите направление")
    .max(100, "Максимум 100 символов"),
  description: z
    .string()
    .trim()
    .max(1000, "Максимум 1000 символов")
    .optional(),
});

type CreatePositionFormData = z.infer<typeof createPositionSchema>;

const initialData: CreatePositionFormData = {
  speciality: "",
  direction: "",
  description: "",
};

export function AddPositionDialog() {
  const [open, setOpen] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<CreatePositionFormData>({
    resolver: zodResolver(createPositionSchema),
    defaultValues: initialData,
  });

  const onSubmit = (data: CreatePositionFormData) => {
    console.log(data);
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
          Добавить должность
        </Button>
      </DialogTrigger>

      <DialogContent className="sm:max-w-lg">
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <DialogHeader>
            <DialogTitle>Новая должность</DialogTitle>
            <DialogDescription>
              Укажите специальность и направление.
            </DialogDescription>
          </DialogHeader>

          <div className="grid gap-4 py-2">
            <div className="grid gap-2">
              <Label htmlFor="position-speciality">Специальность</Label>
              <Input
                id="position-speciality"
                placeholder="Software Engineer"
                {...register("speciality")}
              />
              {errors.speciality && (
                <p className="text-xs text-destructive">{errors.speciality.message}</p>
              )}
            </div>

            <div className="grid gap-2">
              <Label htmlFor="position-direction">Направление</Label>
              <Input
                id="position-direction"
                placeholder="Backend"
                {...register("direction")}
              />
              {errors.direction && (
                <p className="text-xs text-destructive">{errors.direction.message}</p>
              )}
            </div>

            <div className="grid gap-2">
              <Label htmlFor="position-description">
                Описание{" "}
                <span className="text-muted-foreground">(необязательно)</span>
              </Label>
              <Input
                id="position-description"
                placeholder="Краткое описание должности"
                {...register("description")}
              />
              {errors.description && (
                <p className="text-xs text-destructive">{errors.description.message}</p>
              )}
            </div>
          </div>

          <DialogFooter>
            <DialogClose asChild>
              <Button type="button" variant="outline">
                Отмена
              </Button>
            </DialogClose>
            <Button type="submit">
              Добавить
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
