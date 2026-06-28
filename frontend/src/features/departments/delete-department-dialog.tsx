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
import { Trash2, TriangleAlert } from "lucide-react";
import { useState } from "react";
import { useDeleteDepartment } from "./model/use-delete-department";

type Props = {
  department: GetDepartmentDto;
};

export function DeleteDepartmentDialog({ department }: Props) {
  const [open, setOpen] = useState(false);

  const { deleteDepartment, isPending } = useDeleteDepartment();

  const onConfirm = () => {
    deleteDepartment(department.id, {
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
          aria-label={`Удалить ${department.name}`}
        >
          <Trash2 />
        </Button>
      </DialogTrigger>

      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <TriangleAlert className="size-5 text-destructive" />
            Удалить подразделение?
          </DialogTitle>
          <DialogDescription>Это действие нельзя отменить.</DialogDescription>
        </DialogHeader>

        <div className="space-y-3 py-2 text-sm">
          <p>
            Вы собираетесь удалить подразделение{" "}
            <span className="font-medium text-foreground">
              {department.name}
            </span>
            .
          </p>
          <p className="text-muted-foreground">
            После подтверждения оно исчезнет из списка и перестанет быть
            доступным для выбора.
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
