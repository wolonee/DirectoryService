"use client";

import { Spinner } from "@/shared/components/ui/spinner";
import { useLocationsList } from "../../features/locations/model/use-locations-list";
import PageError from "@/shared/components/page-error";
import LocationEmptyState from "../../features/locations/location-empty-state";
import LocationCard from "../../features/locations/location-card";
import LocationStats from "../../entities/locations/ui/location-stats";
import LocationHeader from "../../entities/locations/ui/location-header";
import { Input } from "@/shared/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
import { Search } from "lucide-react";
import { useState } from "react";
import { useDebounce } from "use-debounce";

export default function LocationsList() {
  const [search, setSearch] = useState("");
  const [debouncedSearch] = useDebounce(search, 300);
  const [isActive, setIsActive] = useState<boolean | undefined>(undefined);

  const {
    locations,
    totalCount,
    isLoading,
    error,
    isFetching,
    refetch,
    isFetchingNextPage,
    cursorRef,
  } = useLocationsList({ search: debouncedSearch, isActive });

  if (error) {
    return (
      <PageError
        error={error}
        refetch={refetch}
        isFetching={isFetching}
        name="локации"
      />
    );
  }

  return (
    <main className="min-h-[calc(100vh-4rem)] bg-background text-foreground">
      <div className="mx-auto w-full max-w-7xl px-4 py-8 sm:px-6 sm:py-10 lg:px-8">
        <LocationHeader />

        <LocationStats locations={locations} totalCount={totalCount} />

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

            <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
              <Select
                value={
                  isActive === undefined ? "all" : isActive ? "active" : "inactive"
                }
                onValueChange={(value) => {
                  if (value === "all") setIsActive(undefined);
                  else if (value === "active") setIsActive(true);
                  else setIsActive(false);
                }}
              >
                <SelectTrigger className="w-full sm:w-40">
                  <SelectValue placeholder="Статус" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Все</SelectItem>
                  <SelectItem value="active">Активные</SelectItem>
                  <SelectItem value="inactive">Неактивные</SelectItem>
                </SelectContent>
              </Select>

              <div className="relative w-full lg:max-w-xs">
                <Search className="absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
                <Input
                  type="search"
                  placeholder="Поиск локаций..."
                  className="pl-9"
                  value={search}
                  onChange={(e) => setSearch(e.target.value)}
                />
              </div>
            </div>
          </div>

          <div className="mt-5 grid gap-4 xl:grid-cols-2">
            {isLoading ? (
              <div className="flex h-full items-center justify-center xl:col-span-2">
                <Spinner />
              </div>
            ) : !locations?.length ? (
              <div className="xl:col-span-2">
                <LocationEmptyState search={search} />
              </div>
            ) : (
              locations.map((location) => (
                <LocationCard key={location.id} location={location} />
              ))
            )}
          </div>
        </section>

        <div ref={cursorRef} className="flex justify-center py-4">
          {isFetchingNextPage && <Spinner />}
        </div>
      </div>
    </main>
  );
}
