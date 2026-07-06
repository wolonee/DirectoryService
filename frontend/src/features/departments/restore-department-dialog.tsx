"use client";

import { GetDepartmentDto } from "@/entities/departments/types";
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
import { useRestoreDepartment } from "./model/use-restore-department";

type Props = {
  department: GetDepartmentDto;
};

export function RestoreDepartmentDialog({ department }: Props) {
  const [open, setOpen] = useState(false);

  const { restoreDepartment, isPending } = useRestoreDepartment();

  const onConfirm = () => {
    restoreDepartment(department.id, {
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
          aria-label={`Восстановить ${department.name}`}
        >
          <RotateCcw className="size-4" />
          Восстановить
        </Button>
      </DialogTrigger>

      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <RotateCcw className="size-5 text-sky-400" />
            Восстановить подразделение?
          </DialogTitle>
          <DialogDescription>
            Запись вернется в активный список после подтверждения.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-3 py-2 text-sm">
          <p>
            Вы собираетесь восстановить подразделение{" "}
            <span className="font-medium text-foreground">
              {department.name}
            </span>
            .
          </p>

          <p className="text-muted-foreground">
            После восстановления оно снова будет доступно для выбора и
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
