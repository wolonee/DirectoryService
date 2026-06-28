"use client";

import { GetDepartmentDto } from "@/entities/departments/types";
import { Button } from "@/shared/components/ui/button";
import { Checkbox } from "@/shared/components/ui/checkbox";
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
import { Label } from "@/shared/components/ui/label";
import { Spinner } from "@/shared/components/ui/spinner";
import { Pencil } from "lucide-react";
import { useState } from "react";
import { useLocationsSelect } from "./model/use-locations-select";
import { useUpdateDepartmentLocations } from "./model/use-update-department-locations";

type Props = {
  department: GetDepartmentDto;
};

export function UpdateDepartmentDialog({ department }: Props) {
  const [open, setOpen] = useState(false);
  const [selectedLocationIds, setSelectedLocationIds] = useState<string[]>([]);

  const {
    locations,
    isLoading: isLoadingLocations,
    isFetchingNextPage: isFetchingNextLocations,
    cursorRef: locationCursorRef,
  } = useLocationsSelect();

  const { updateDepartmentLocations, isPending, error, commonError, resetError } =
    useUpdateDepartmentLocations();

  const toggleLocation = (id: string) => {
    setSelectedLocationIds((current) =>
      current.includes(id)
        ? current.filter((l) => l !== id)
        : [...current, id],
    );
  };

  const onSubmit = () => {
    resetError();

    updateDepartmentLocations(
      {
        departmentId: department.id,
        locationsIds: selectedLocationIds,
      },
      {
        onSuccess: () => setOpen(false),
      },
    );
  };

  const handleOpenChange = (isOpen: boolean) => {
    setOpen(isOpen);

    if (!isOpen) {
      resetError();
      setSelectedLocationIds([]);
    }
  };

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogTrigger asChild>
        <Button
          variant="ghost"
          size="icon"
          type="button"
          aria-label={`Редактировать ${department.name}`}
        >
          <Pencil />
        </Button>
      </DialogTrigger>

      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>Локации подразделения</DialogTitle>
          <DialogDescription>
            Выберите локации для «{department.name}». Список заменит текущие
            привязки.
          </DialogDescription>
        </DialogHeader>

        <div className="grid gap-2 py-2">
          <Label>Локации</Label>
          <div className="max-h-56 overflow-y-auto rounded-md border border-input p-3">
            {isLoadingLocations ? (
              <div className="flex justify-center py-2">
                <Spinner />
              </div>
            ) : (
              <div className="grid gap-2">
                {locations.map((location) => (
                  <div key={location.id} className="flex items-center gap-2">
                    <Checkbox
                      id={`edit-location-${location.id}`}
                      checked={selectedLocationIds.includes(location.id)}
                      onCheckedChange={() => toggleLocation(location.id)}
                    />
                    <Label
                      htmlFor={`edit-location-${location.id}`}
                      className="cursor-pointer font-normal"
                    >
                      {location.name}{" "}
                      <span className="text-muted-foreground">
                        · {location.city}, {location.country}
                      </span>
                    </Label>
                  </div>
                ))}
                <div ref={locationCursorRef} className="flex justify-center py-1">
                  {isFetchingNextLocations && <Spinner />}
                </div>
              </div>
            )}
          </div>
        </div>

        {error && <p className="text-sm text-destructive">{error.message}</p>}

        {commonError && (
          <p className="text-sm text-destructive">
            Не удалось обновить локации. Попробуйте позже.
          </p>
        )}

        <DialogFooter>
          <DialogClose asChild>
            <Button type="button" variant="outline">
              Отмена
            </Button>
          </DialogClose>
          <Button
            type="button"
            onClick={onSubmit}
            disabled={isPending || selectedLocationIds.length === 0}
          >
            {isPending ? "Сохраняем..." : "Сохранить"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
