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
import { RotateCcw } from "lucide-react";
import { useState } from "react";
import { useRestoreLocation } from "./model/use-restore-location";

type Props = {
  location: GetLocationDto;
};

export function RestoreLocationDialog({ location }: Props) {
  const [open, setOpen] = useState(false);

  const { restoreLocation, isPending } = useRestoreLocation();

  const onConfirm = () => {
    restoreLocation(location.id, {
      onSuccess: () => setOpen(false),
    });
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button
          type="button"
          variant="outline"
          size="sm"
          aria-label={`Восстановить ${location.name}`}
        >
          <RotateCcw className="size-4" />
          Восстановить
        </Button>
      </DialogTrigger>

      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <RotateCcw className="size-5 text-sky-400" />
            Восстановить локацию?
          </DialogTitle>
          <DialogDescription>
            Запись вернется в активный список после подтверждения.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-3 py-2 text-sm">
          <p>
            Вы собираетесь восстановить локацию{" "}
            <span className="font-medium text-foreground">{location.name}</span>{" "}
            ({location.country}, {location.city}, {location.street}).
          </p>

          <p className="text-muted-foreground">
            После восстановления она снова будет доступна для выбора и
            отображения в связанных сценариях.
          </p>
        </div>

        <DialogFooter>
          <DialogClose asChild>
            <Button type="button" variant="outline" disabled={isPending}>
              Отмена
            </Button>
          </DialogClose>
          <Button type="button" onClick={onConfirm} disabled={isPending}>
            {isPending ? "Восстанавливаем..." : "Восстановить"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
