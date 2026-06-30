"use client";

import { useDebounce } from "use-debounce";
import { FolderTree } from "lucide-react";

import PageError from "@/shared/components/page-error";
import { Spinner } from "@/shared/components/ui/spinner";
import { useDepartmentsList } from "./model/use-departments-list";
import { DepartmentFilters } from "./department-filters";
import DepartmentCard from "./department-card";
import { useGetDepartmentFilter } from "./model/departments-filter-store";
import { DepartmentHeader } from "@/entities/departments/ui/department-header";

export default function DepartmentsList() {
  const { search, isActive, sortBy, sortDir } = useGetDepartmentFilter();

  const [debouncedSearch] = useDebounce(search, 300);

  const {
    departments,
    totalCount,
    isLoading,
    error,
    isFetching,
    refetch,
    isFetchingNextPage,
    cursorRef,
  } = useDepartmentsList({ search: debouncedSearch, isActive, sortBy, sortDir });

  if (error) {
    return (
      <PageError
        error={error}
        refetch={refetch}
        isFetching={isFetching}
        name="подразделения"
      />
    );
  }

  return (
    <main className="min-h-[calc(100vh-4rem)] bg-background text-foreground">
      <div className="mx-auto w-full max-w-7xl px-4 py-8 sm:px-6 sm:py-10 lg:px-8">

        <DepartmentHeader />

        <section className="mt-8" aria-labelledby="departments-list-title">
          <div>
            <h2 id="departments-list-title" className="text-lg font-semibold">
              Все подразделения
            </h2>
            <p className="mt-1 text-sm text-muted-foreground">
              Найдено: {totalCount ?? 0}
            </p>
          </div>

          <DepartmentFilters />

          <div className="mt-5 grid gap-4 md:grid-cols-2 xl:grid-cols-3">
            {isLoading ? (
              <div className="flex h-full items-center justify-center md:col-span-2 xl:col-span-3">
                <Spinner />
              </div>
            ) : !departments?.length ? (
              <div className="flex min-h-72 flex-col items-center justify-center rounded-xl border border-dashed border-border p-6 text-center md:col-span-2 xl:col-span-3">
                <FolderTree className="size-9 text-muted-foreground" />
                <p className="mt-4 font-medium">
                  {search
                    ? `По запросу «${search}» ничего не найдено`
                    : "Подразделений пока нет"}
                </p>
              </div>
            ) : (
              departments.map((department) => (
                <DepartmentCard key={department.id} department={department} />
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
