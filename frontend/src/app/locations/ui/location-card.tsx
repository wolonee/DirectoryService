import { GetLocationDto } from "@/entities/locations/types";
import { Button } from "@/shared/components/ui/button";
import {
  Card,
  CardHeader,
  CardTitle,
  CardDescription,
  CardAction,
  CardContent,
} from "@/shared/components/ui/card";
import { Building2, MoreHorizontal, MapPin, Clock3 } from "lucide-react";

type Props = {
  location: GetLocationDto;
};

export default function LocationCard({ location }: Props) {
  return (
    <Card key={location.id} className="transition-colors hover:bg-muted/30">
      <CardHeader className="grid-cols-[auto_1fr_auto] items-start gap-x-4">
        <div
          className="relative flex size-11 items-center justify-center rounded-xl bg-muted ring-1 ring-border"
          aria-hidden
        >
          <Building2 className="size-5 text-muted-foreground" />
        </div>

        <div className="min-w-0">
          <CardTitle className="truncate text-base">{location.name}</CardTitle>
          <CardDescription className="mt-1 flex flex-wrap items-center gap-x-2 gap-y-1">
            <span>
              {location.city}, {location.country}
            </span>
            <span aria-hidden>·</span>
            <span className="font-mono text-xs">{location.id}</span>
          </CardDescription>
        </div>

        <CardAction className="static col-auto row-auto">
          <Button
            variant="ghost"
            size="icon"
            type="button"
            aria-label={`Действия для ${location.name}`}
          >
            <MoreHorizontal />
          </Button>
        </CardAction>
      </CardHeader>

      <CardContent>
        <div className="grid gap-3 text-sm text-muted-foreground sm:grid-cols-2">
          <div className="flex items-start gap-2.5 sm:col-span-2">
            <MapPin className="mt-0.5 size-4 shrink-0" />
            <span className="text-foreground">
              {location.country}, {location.city}, {location.street}
            </span>
          </div>
          <div className="flex items-center gap-2.5">
            <Clock3 className="size-4 shrink-0" />
            <span>{location.timezone}</span>
          </div>
          <div className="flex items-center gap-2.5">
            <Building2 className="size-4 shrink-0" />
            <span>
              {location.countDepartments > 0
                ? `${location.countDepartments} подразделений`
                : "Без подразделений"}
            </span>
          </div>
        </div>

        <div className="mt-5 flex items-center justify-between border-t border-border/70 pt-4">
          <span className="text-xs text-muted-foreground">
            Создана{" "}
            {new Intl.DateTimeFormat("ru-RU", {
              dateStyle: "medium",
            }).format(new Date(location.createdAt))}
          </span>
          <span className="inline-flex items-center rounded-full bg-sky-500/10 px-2.5 py-1 text-xs font-medium text-sky-400">
            {location.timezone}
          </span>
        </div>
      </CardContent>
    </Card>
  );
}
