"use client";

import { Spinner } from "@/shared/components/ui/spinner";
import { useLocationsList } from "../../features/locations/model/use-locations-list";
import PageError from "@/shared/components/page-error";
import LocationEmptyState from "../../features/locations/location-empty-state";
import LocationCard from "./ui/location-card";
import LocationStats from "./ui/location-stats";
import LocationHeader from "./ui/location-header";
// import { LocationPagination } from "@/features/locations/location-pagination";


export default function LocationsPage() {
  const {
    locations,
    totalCount,
    isLoading,
    error,
    isFetching,
    refetch,
    isFetchingNextPage,
    cursorRef,
  } = useLocationsList();

  if (isLoading) {
    return (
      <div className="flex h-full items-center justify-center">
        <Spinner />
      </div>
    );
  }

  if (error) {
    return (
      <PageError error={error} refetch={refetch} isFetching={isFetching} name="локации"/>
    );
  }

  if (!locations?.length) {
    return (
      <LocationEmptyState/>
    );
  }

  return (
    <main className="min-h-[calc(100vh-4rem)] bg-background text-foreground">
      <div className="mx-auto w-full max-w-7xl px-4 py-8 sm:px-6 sm:py-10 lg:px-8">

        <LocationHeader />

        <LocationStats locations={locations} totalCount={totalCount}/>

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
              <LocationCard key={location.id} location={location} />
            ))}
          </div>
        </section>

        <div ref={cursorRef} className="flex justify-center py-4">{isFetchingNextPage && <Spinner />}</div>
      </div>
    </main>
  );
}
