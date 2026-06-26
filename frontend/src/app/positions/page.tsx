"use client";

import { Spinner } from "@/shared/components/ui/spinner";
import { usePositionsList } from "@/features/positions/model/use-position-list";
import { PostitionEmptyState } from "@/features/positions/position-empty-state";
import PageError from "@/shared/components/page-error";
import PositionStats from "../../entities/positions/ui/position-stats";
import PositionCard from "../../features/positions/position-card";
import PositionHeader from "../../entities/positions/ui/position-header";

// const mockPositions: GetPositionDto[] = [
//   { id: "1", speciality: "Software Engineer", direction: "Backend", createdAt: "2024-01-15T10:00:00Z" },
//   { id: "2", speciality: "Product Designer", direction: "Design", createdAt: "2024-02-20T10:00:00Z" },
//   { id: "3", speciality: "DevOps Engineer", direction: "Infrastructure", createdAt: "2024-03-10T10:00:00Z" },
// ];

export default function PositionsPage() {
  const {
    positions,
    totalCount,
    isLoading,
    error,
    isFetchingNextPage,
    cursorRef,
    refetch,
    isFetching,

  } = usePositionsList();

  if (isLoading) {
    return (
      <div className="flex h-full items-center justify-center">
        <Spinner />
      </div>
    );
  }

  if (error) {
    return (
      <PageError error={error} refetch={refetch} isFetching={isFetching} name={"позиции"}/>
    )
  }

  if (!positions?.length) {
    return (
      <PostitionEmptyState />
    )
  }

  return (
    <main className="min-h-[calc(100vh-4rem)] bg-background text-foreground">
      <div className="mx-auto w-full max-w-7xl px-4 py-8 sm:px-6 sm:py-10 lg:px-8">

        <PositionHeader />

        <PositionStats positions={positions} totalCount={totalCount} />

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
              <PositionCard key={position.id} position={position}/>
            ))}
          </div>
        </section>

        <div ref={cursorRef} className="flex justify-center py-4">{isFetchingNextPage && <Spinner />}</div>
      </div>
    </main>
  );
}
