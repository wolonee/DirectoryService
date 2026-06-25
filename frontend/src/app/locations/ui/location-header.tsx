import { AddLocationDialog } from "@/features/locations/create-location-dialog";
import { MapPin } from "lucide-react";

export default function LocationHeader () {
  return (
    <section className="flex flex-col gap-6 border-b border-border/70 pb-8 sm:flex-row sm:items-end sm:justify-between">
      <div>
        <div className="mb-4 flex size-11 items-center justify-center rounded-xl bg-sky-500/10 text-sky-400 ring-1 ring-sky-400/20">
          <MapPin className="size-5" />
        </div>
        <p className="text-sm font-medium text-muted-foreground">
          Корпоративный справочник
        </p>
        <h1 className="mt-1 text-3xl font-semibold tracking-tight sm:text-4xl">
          Locations
        </h1>
        <p className="mt-3 max-w-2xl text-sm leading-6 text-muted-foreground sm:text-base">
          Офисы, филиалы и рабочие площадки организации.
        </p>
      </div>

      <div>
        <AddLocationDialog />
      </div>
    </section>
  );
}
