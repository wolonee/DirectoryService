"use client";

import { DepartmentTreeNode } from "@/entities/departments/types";
import { DepartmentSelect, NO_PARENT } from "@/entities/departments/features/department-select";
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
import { ArrowRight, FolderInput } from "lucide-react";
import { useState } from "react";
import { useUpdateDepartmentParent } from "../model/use-update-department-parent";
import { useDepartmentDescendants } from "@/entities/departments/model/use-department-descendants";

type Props = {
  node: DepartmentTreeNode;
};

export function UpdateDepartmentParentDialog({ node }: Props) {
  const [open, setOpen] = useState(false);
  const [newParentId, setNewParentId] = useState("");

  // Весь поддерев переносимого узла — грузим при открытии диалога, чтобы исключить из выбора
  // (нельзя перенести узел ни в одного из его потомков — иначе цикл).
  const { descendantIds } = useDepartmentDescendants(node.id, open);

  const parentNames = useDepartmentNames(node.parentId ? [node.parentId] : []);
  const currentParent = node.parentId
    ? (parentNames.get(node.parentId) ?? "…")
    : "Корень";

  // Имя выбранного нового родителя для preview ("none" = корень, "" = ещё не выбран).
  const newParentNames = useDepartmentNames(
    newParentId && newParentId !== NO_PARENT ? [newParentId] : [],
  );
  const hasSelection = newParentId !== "";
  const newParent =
    newParentId === NO_PARENT
      ? "Корень"
      : (newParentNames.get(newParentId) ?? "…");

  const onOpenChange = (next: boolean) => {
    setOpen(next);
    if (!next) {
      setNewParentId("");
      resetError();
    }
  };

  const onSelectParent = (value: string) => {
    setNewParentId(value);
    resetError();
  };

  const { updateDepartmentParent, isPending, error, commonError, resetError } =
    useUpdateDepartmentParent();

  const fieldErrors = error?.fieldErrors ?? [];
  const operationErrors =
    error?.errorList.filter((e) => !e.invalidField) ?? [];

  const onSubmit = () => {
    updateDepartmentParent({ departmentId: node.id, newParentId }, {
      onSuccess: () => {
        setOpen(false);
        setNewParentId("");
      },
    });
  }

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
              onChange={onSelectParent}
              placeholder="Без родителя (в корень)"
              excludeId={[
                node.id,
                node.parentId ?? "",
                ...descendantIds,
              ].filter(Boolean)}
            />

            {fieldErrors.map((e) => (
              <p key={e.code} className="text-sm text-destructive">
                {e.message}
              </p>
            ))}
          </div>

          {hasSelection && (
            <div className="rounded-md bg-muted/50 p-3">
              <span className="text-muted-foreground">Предпросмотр</span>
              <div className="mt-1.5 flex items-center gap-2 font-medium text-foreground">
                <span className="truncate">{currentParent}</span>
                <ArrowRight className="size-4 shrink-0 text-muted-foreground" />
                <span className="truncate">{newParent}</span>
              </div>
            </div>
          )}

          {(operationErrors.length > 0 || commonError) && (
            <div className="space-y-1 rounded-md border border-destructive/30 bg-destructive/10 p-3 text-destructive">
              {operationErrors.map((e) => (
                <p key={e.code}>{e.message}</p>
              ))}
              {commonError && (
                <p>Не удалось перенести подразделение. Попробуйте позже.</p>
              )}
            </div>
          )}
        </div>

        <DialogFooter>
          <DialogClose asChild>
            <Button type="button" variant="outline">
              Отмена
            </Button>
          </DialogClose>
          <Button type="button" onClick={onSubmit} disabled={isPending}>
            Перенести
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
