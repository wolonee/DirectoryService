import { AddPositionDialog } from "@/features/positions/create-position-dialog";
import { BriefcaseBusiness } from "lucide-react";

export default function PositionHeader() {
  return (
    <section className="flex flex-col gap-6 border-b border-border/70 pb-8 sm:flex-row sm:items-end sm:justify-between">
      <div>
        <div className="mb-4 flex size-11 items-center justify-center rounded-xl bg-violet-500/10 text-violet-400 ring-1 ring-violet-400/20">
          <BriefcaseBusiness className="size-5" />
        </div>
        <p className="text-sm font-medium text-muted-foreground">
          Корпоративный справочник
        </p>
        <h1 className="mt-1 text-3xl font-semibold tracking-tight sm:text-4xl">
          Positions
        </h1>
        <p className="mt-3 max-w-2xl text-sm leading-6 text-muted-foreground sm:text-base">
          Должности, роли и зоны ответственности в организации.
        </p>
      </div>
      <div>
        <AddPositionDialog />
      </div>
    </section>
  );
}
