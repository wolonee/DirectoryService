"use client";

import { useDebounce } from "use-debounce";
import { Building2, FolderTree } from "lucide-react";

import PageError from "@/shared/components/page-error";
import { Spinner } from "@/shared/components/ui/spinner";
import { useDepartmentsList } from "./model/use-departments-list";
import { DepartmentFilters } from "./department-filters";
import DepartmentCard from "./department-card";
import { AddDepartmentDialog } from "./create-department-dialog";
import { useGetDepartmentFilter } from "./model/departments-filter-store";

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
        <section className="flex flex-col gap-6 border-b border-border/70 pb-8 sm:flex-row sm:items-end sm:justify-between">
          <div>
            <div className="mb-4 flex size-11 items-center justify-center rounded-xl bg-violet-500/10 text-violet-400 ring-1 ring-violet-400/20">
              <Building2 className="size-5" />
            </div>
            <p className="text-sm font-medium text-muted-foreground">
              Корпоративный справочник
            </p>
            <h1 className="mt-1 text-3xl font-semibold tracking-tight sm:text-4xl">
              Departments
            </h1>
            <p className="mt-3 max-w-2xl text-sm leading-6 text-muted-foreground sm:text-base">
              Структура подразделений и команд организации.
            </p>
          </div>

          <AddDepartmentDialog />
        </section>

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
