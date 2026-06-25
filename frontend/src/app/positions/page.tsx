"use client";

import { BriefcaseBusiness, Clock3, Building2 } from "lucide-react";
import { Spinner } from "@/shared/components/ui/spinner";
import { Button } from "@/shared/components/ui/button";
import { AddPositionDialog } from "@/features/positions/create-position-dialog";
import {
  Card,
  CardHeader,
  CardTitle,
  CardDescription,
  CardAction,
  CardContent,
} from "@/shared/components/ui/card";
import { GetPositionDto } from "@/entities/positions/types";

const mockPositions: GetPositionDto[] = [
  { id: "1", speciality: "Software Engineer", direction: "Backend", createdAt: "2024-01-15T10:00:00Z" },
  { id: "2", speciality: "Product Designer", direction: "Design", createdAt: "2024-02-20T10:00:00Z" },
  { id: "3", speciality: "DevOps Engineer", direction: "Infrastructure", createdAt: "2024-03-10T10:00:00Z" },
];

export default function PositionsPage() {
  const positions = mockPositions;
  const totalCount = mockPositions.length;
  const isLoading = false;
  const isFetchingNextPage = false;

  if (isLoading) {
    return (
      <div className="flex h-full items-center justify-center">
        <Spinner />
      </div>
    );
  }

  if (!positions.length) {
    return (
      <div className="flex min-h-[calc(100vh-4rem)] items-center justify-center p-6">
        <div className="flex flex-col items-center gap-3 text-center">
          <BriefcaseBusiness className="size-8 text-muted-foreground" />
          <div>
            <p className="font-medium">Должностей пока нет</p>
          </div>
          <AddPositionDialog />
        </div>
      </div>
    );
  }

  return (
    <main className="min-h-[calc(100vh-4rem)] bg-background text-foreground">
      <div className="mx-auto w-full max-w-7xl px-4 py-8 sm:px-6 sm:py-10 lg:px-8">

        {/* Хедер */}
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

        {/* Статистика */}
        <section
          aria-label="Сводка по должностям"
          className="mt-8 grid grid-cols-2 gap-px overflow-hidden rounded-xl bg-border/70 ring-1 ring-border/70 sm:grid-cols-3"
        >
          <div className="bg-card p-4 sm:p-5">
            <p className="text-xs font-medium text-muted-foreground">Всего должностей</p>
            <p className="mt-2 text-2xl font-semibold tabular-nums">{totalCount}</p>
          </div>
          <div className="bg-card p-4 sm:p-5">
            <p className="text-xs font-medium text-muted-foreground">На странице</p>
            <p className="mt-2 text-2xl font-semibold tabular-nums">{positions.length}</p>
          </div>
          <div className="bg-card p-4 sm:p-5">
            <p className="text-xs font-medium text-muted-foreground">Направлений</p>
            <p className="mt-2 text-2xl font-semibold tabular-nums">
              {new Set(positions.map((p) => p.direction)).size}
            </p>
          </div>
        </section>

        {/* Список */}
        <section className="mt-8" aria-labelledby="positions-list-title">
          <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
            <div>
              <h2 id="positions-list-title" className="text-lg font-semibold">
                Все должности
              </h2>
              <p className="mt-1 text-sm text-muted-foreground">
                {totalCount} позиций
              </p>
            </div>
          </div>

          <div className="mt-5 grid gap-4 xl:grid-cols-2">
            {positions.map((position) => (
              <Card key={position.id} className="transition-colors hover:bg-muted/30">
                <CardHeader className="grid-cols-[auto_1fr_auto] items-start gap-x-4">
                  <div
                    className="relative flex size-11 items-center justify-center rounded-xl bg-violet-500/10 text-violet-400 ring-1 ring-violet-400/20"
                    aria-hidden
                  >
                    <BriefcaseBusiness className="size-5" />
                  </div>
                  <div className="min-w-0">
                    <CardTitle className="truncate text-base">{position.speciality}</CardTitle>
                    <CardDescription className="mt-1">
                      {position.direction}
                    </CardDescription>
                  </div>
                  <CardAction className="static col-auto row-auto">
                    <Button
                      variant="ghost"
                      size="icon"
                      type="button"
                      aria-label={`Действия для ${position.speciality}`}
                    >
                      <Building2 />
                    </Button>
                  </CardAction>
                </CardHeader>

                <CardContent>
                  <div className="grid gap-3 text-sm text-muted-foreground sm:grid-cols-2">
                    <div className="flex items-center gap-2.5">
                      <Building2 className="size-4 shrink-0" />
                      <span>{position.direction}</span>
                    </div>
                    <div className="flex items-center gap-2.5">
                      <Clock3 className="size-4 shrink-0" />
                      <span>
                        {new Intl.DateTimeFormat("ru-RU", { dateStyle: "medium" }).format(
                          new Date(position.createdAt)
                        )}
                      </span>
                    </div>
                  </div>

                  <div className="mt-5 flex items-center justify-between border-t border-border/70 pt-4">
                    <span className="text-xs text-muted-foreground">
                      Создана{" "}
                      {new Intl.DateTimeFormat("ru-RU", { dateStyle: "medium" }).format(
                        new Date(position.createdAt)
                      )}
                    </span>
                    <span className="inline-flex items-center rounded-full bg-violet-500/10 px-2.5 py-1 text-xs font-medium text-violet-400">
                      {position.direction}
                    </span>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </section>

        <div className="flex justify-center py-4">
          {isFetchingNextPage && <Spinner />}
        </div>

      </div>
    </main>
  );
}
