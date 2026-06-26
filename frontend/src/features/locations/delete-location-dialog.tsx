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
import { Trash2, TriangleAlert } from "lucide-react";
import { useState } from "react";
import { useDeleteLocation } from "./model/use-delete-location";

type Props = {
  location: GetLocationDto;
};

export function DeleteLocationDialog({ location }: Props) {
  const [open, setOpen] = useState(false);

  const { deleteLocation, isPending } = useDeleteLocation();

  const onConfirm = () => {
    deleteLocation(location.id, {
      onSuccess: () => setOpen(false),
    });
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button
          variant="ghost"
          size="icon"
          type="button"
          aria-label={`Удалить ${location.name}`}
        >
          <Trash2 />
        </Button>
      </DialogTrigger>

      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <TriangleAlert className="size-5 text-destructive" />
            Удалить локацию?
          </DialogTitle>
          <DialogDescription>
            Это действие нельзя отменить.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-3 py-2 text-sm">
          <p>
            Вы собираетесь удалить локацию{" "}
            <span className="font-medium text-foreground">{location.name}</span>{" "}
            ({location.country}, {location.city}, {location.street}).
          </p>

          {location.countDepartments > 0 && (
            <p className="rounded-md bg-destructive/10 px-3 py-2 text-destructive">
              К этой локации привязано подразделений: {location.countDepartments}.
              После удаления они потеряют связь с ней.
            </p>
          )}

          <p className="text-muted-foreground">
            После подтверждения локация исчезнет из списка и перестанет быть
            доступной для выбора.
          </p>
        </div>

        <DialogFooter>
          <DialogClose asChild>
            <Button type="button" variant="outline" disabled={isPending}>
              Отмена
            </Button>
          </DialogClose>
          <Button
            type="button"
            variant="destructive"
            onClick={onConfirm}
            disabled={isPending}
          >
            {isPending ? "Удаляем..." : "Удалить"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
