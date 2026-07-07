"use client";

import { DepartmentTreeNode } from "@/entities/departments/types";
import { DepartmentSelect } from "@/entities/departments/features/department-select";
import { useDepartmentNames } from "@/entities/departments/model/use-department-names";
import { Button } from "@/shared/components/ui/button";
import { Label } from "@/shared/components/ui/label";
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
import { FolderInput } from "lucide-react";
import { useState } from "react";

type Props = {
  node: DepartmentTreeNode;
};

export function MoveDepartmentDialog({ node }: Props) {
  const [open, setOpen] = useState(false);

  // Только UI: выбранный новый родитель. Мутацию/валидацию повесим позже.
  const [newParentId, setNewParentId] = useState("");

  // Имя текущего родителя (display). Если parentId === null — это корень.
  const parentNames = useDepartmentNames(node.parentId ? [node.parentId] : []);
  const currentParent = node.parentId
    ? (parentNames.get(node.parentId) ?? "…")
    : "Корень";

  const onOpenChange = (next: boolean) => {
    setOpen(next);
    if (!next) setNewParentId(""); // закрытие без сохранения сбрасывает выбор
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogTrigger asChild>
        <Button
          variant="ghost"
          size="icon-sm"
          type="button"
          aria-label={`Перенести ${node.name}`}
          onClick={(e) => e.stopPropagation()}
          className="opacity-0 transition-opacity group-hover:opacity-100 focus-visible:opacity-100"
        >
          <FolderInput className="text-muted-foreground" />
        </Button>
      </DialogTrigger>

      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Перенести подразделение</DialogTitle>
          <DialogDescription>
            Выберите нового родителя для этого подразделения.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4 py-2 text-sm">
          <div className="space-y-1">
            <span className="text-muted-foreground">Подразделение</span>
            <p className="font-medium text-foreground">{node.name}</p>
          </div>

          <div className="space-y-1">
            <span className="text-muted-foreground">Текущий родитель</span>
            <p className="font-medium text-foreground">{currentParent}</p>
          </div>

          <div className="space-y-2">
            <Label>Новый родитель</Label>
            <DepartmentSelect
              value={newParentId}
              onChange={setNewParentId}
              placeholder="Без родителя (в корень)"
            />
          </div>
        </div>

        <DialogFooter>
          <DialogClose asChild>
            <Button type="button" variant="outline">
              Отмена
            </Button>
          </DialogClose>
          <Button type="button" onClick={() => setOpen(false)}>
            Перенести
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
