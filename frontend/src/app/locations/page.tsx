"use client";

import {
  Building2,
  Clock3,
  MapPin,
  MoreHorizontal,
} from "lucide-react";

import { Button } from "@/shared/components/ui/button";
import {
  Card,
  CardAction,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card";

import { useState } from "react";
import { Spinner } from "@/shared/components/ui/spinner";
import { useLocationsList } from "../../features/locations/model/use-locations-list";
import { AddLocationDialog } from "@/features/locations/create-location-dialog";
import { LocationPagination } from "@/features/locations/location-pagination";



export default function LocationsPage() {
  const [open, setOpen] = useState(false);
  const [page, setPage] = useState(1);

  const { locations, totalCount, totalPages, isLoading, error, isFetching, refetch } = useLocationsList({ page });

  if (isLoading) {
    return (
      <div className="flex h-full items-center justify-center">
        <Spinner />
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex min-h-[calc(100vh-4rem)] items-center justify-center p-6">
        <Card className="max-w-md">
          <CardHeader>
            <CardTitle>Не удалось загрузить подразделения</CardTitle>
            <CardDescription>{error.message}</CardDescription>
            <Button type="button"
            onClick={() => refetch()}
            disabled={isFetching}
            className="mt-4">
              Повторить
            </Button>
          </CardHeader>
        </Card>
      </div>
    );
  }

  if (locations?.length === 0) {
    return (
      <div className="flex min-h-[calc(100vh-4rem)] items-center justify-center p-6">
        <div className="flex flex-col items-center gap-3 text-center">
          <MapPin className="size-8 text-muted-foreground" />
          <div>
            <p className="font-medium">Локаций пока нет</p>
            <p className="mt-1 text-sm text-muted-foreground">
              Добавьте первую локацию, чтобы она появилась в справочнике.
            </p>
          </div>
          <AddLocationDialog
            open={open}
            setOpen={setOpen}
            onCreated={() => setPage(1)}
          />
        </div>
      </div>
    );
  }

  const departmentsCount = 0
  const timezonesCount = new Set(
    locations?.map((location) => location.timezone)
  ).size;

  return (
    <main className="min-h-[calc(100vh-4rem)] bg-background text-foreground">
      <div className="mx-auto w-full max-w-7xl px-4 py-8 sm:px-6 sm:py-10 lg:px-8">
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
            <AddLocationDialog
              open={open}
              setOpen={setOpen}
              onCreated={() => setPage(1)}
            />
          </div>
        </section>

        <section
          aria-label="Сводка по локациям"
          className="grid grid-cols-2 gap-px overflow-hidden rounded-xl bg-border/70 ring-1 ring-border/70 sm:grid-cols-4"
        >
          <div className="bg-card p-4 sm:p-5">
            <p className="text-xs font-medium text-muted-foreground">
              Всего локаций
            </p>
            <p className="mt-2 text-2xl font-semibold tabular-nums">
              {totalCount}
            </p>
          </div>
          <div className="bg-card p-4 sm:p-5">
            <p className="text-xs font-medium text-muted-foreground">
              На странице
            </p>
            <p className="mt-2 text-2xl font-semibold tabular-nums">
              {locations?.length}
            </p>
          </div>
          <div className="bg-card p-4 sm:p-5">
            <p className="text-xs font-medium text-muted-foreground">
              Подразделений
            </p>
            <p className="mt-2 text-2xl font-semibold tabular-nums">
              {departmentsCount}
            </p>
          </div>
          <div className="bg-card p-4 sm:p-5">
            <p className="text-xs font-medium text-muted-foreground">
              Часовых поясов
            </p>
            <p className="mt-2 text-2xl font-semibold tabular-nums">
              {timezonesCount}
            </p>
          </div>
        </section>

        <section className="mt-8" aria-labelledby="locations-list-title">
          <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
            <div>
              <h2 id="locations-list-title" className="text-lg font-semibold">
                Все локации
              </h2>
              <p className="mt-1 text-sm text-muted-foreground">
                {totalCount} площадок
              </p>
            </div>

          </div>

          <div className="mt-5 grid gap-4 xl:grid-cols-2">
            {locations?.map((location) => (
              <Card
                key={location.id}
                className="transition-colors hover:bg-muted/30"
              >
                <CardHeader className="grid-cols-[auto_1fr_auto] items-start gap-x-4">
                  <div
                    className="relative flex size-11 items-center justify-center rounded-xl bg-muted ring-1 ring-border"
                    aria-hidden
                  >
                    <Building2 className="size-5 text-muted-foreground" />
                  </div>

                  <div className="min-w-0">
                    <CardTitle className="truncate text-base">
                      {location.name}
                    </CardTitle>
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
            ))}
          </div>
          
          {totalPages && (
            <LocationPagination
              page={page}
              totalPages={totalPages}
              onPageChange={setPage}
            />
          )}
        </section>
      </div>
    </main>
  );
}
