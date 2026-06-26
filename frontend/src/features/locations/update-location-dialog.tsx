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
import { Pencil } from "lucide-react";
import { useState } from "react";

type Props = {
  location: GetLocationDto;
};

export function UpdateLocationDialog({ location }: Props) {
  const [open, setOpen] = useState(false);

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button
          variant="ghost"
          size="icon"
          type="button"
          aria-label={`Редактировать ${location.name}`}
        >
          <Pencil />
        </Button>
      </DialogTrigger>

      <DialogContent className="sm:max-w-lg">
        <form className="space-y-4">
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
                defaultValue={location.name}
              />
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="grid gap-2">
                <Label htmlFor="update-location-country">Страна</Label>
                <Input
                  id="update-location-country"
                  placeholder="Россия"
                  defaultValue={location.country}
                />
              </div>

              <div className="grid gap-2">
                <Label htmlFor="update-location-city">Город</Label>
                <Input
                  id="update-location-city"
                  placeholder="Москва"
                  defaultValue={location.city}
                />
              </div>
            </div>

            <div className="grid gap-2">
              <Label htmlFor="update-location-street">Улица и дом</Label>
              <Input
                id="update-location-street"
                placeholder="ул. Ленина, 1"
                defaultValue={location.street}
              />
            </div>

            <div className="grid gap-2">
              <Label htmlFor="update-location-timezone">Часовой пояс</Label>
              <Input
                id="update-location-timezone"
                placeholder="Europe/Moscow"
                defaultValue={location.timezone}
              />
            </div>
          </div>

          <DialogFooter>
            <DialogClose asChild>
              <Button type="button" variant="outline">
                Отмена
              </Button>
            </DialogClose>
            <Button type="submit">Сохранить</Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
