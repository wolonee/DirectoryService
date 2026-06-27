// "use client";

// import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
// import {
//   Building2,
//   CalendarDays,
//   ChevronRight,
//   FolderTree,
//   Plus,
// } from "lucide-react";
// import { useState } from "react";

// import { departmentsApi } from "@/entities/departments/api";
// import { Button } from "@/shared/components/ui/button";
// import {
//   Card,
//   CardContent,
//   CardDescription,
//   CardHeader,
//   CardTitle,
// } from "@/shared/components/ui/card";
// import {
//   Pagination,
//   PaginationContent,
//   PaginationEllipsis,
//   PaginationItem,
//   PaginationLink,
//   PaginationNext,
//   PaginationPrevious,
// } from "@/shared/components/ui/pagination";
// import { Spinner } from "@/shared/components/ui/spinner";
// import { getVisiblePages } from "@/shared/lib/get-visible-pages";

// const PAGE_SIZE = 2;

// function getPathParts(path: string) {
//   return path.split(/[./\\>]+/).filter(Boolean);
// }

export default function DepartmentsPage() {
  // const queryClient = useQueryClient();
  // const [page, setPage] = useState(1);

  // const { data, error, isLoading, isFetching, refetch } = useQuery(
  //   getDepartmentsQueryOptions(page, PAGE_SIZE)
  // );

  // const { mutate: createDepartment } = useMutation({
  //   mutationFn: () => departmentsApi.createDepartment({
  //        name: "Отдел ", 
  //        identifier: "darch",
  //        parentId: null,
  //        locationIds: []
  //       }),
  //   onSettled: () => {queryClient.invalidateQueries({ queryKey: ["departments"] })},
  // });

  // const departments = data?.items ?? [];
  // const totalCount = data?.totalCount ?? 0;
  // const totalPages = data?.totalPages ?? 0;

  // if (isLoading) {
  //   return (
  //     <div className="flex min-h-[calc(100vh-4rem)] items-center justify-center">
  //       <Spinner />
  //     </div>
  //   );
  // }

  // if (error) {
  //   return (
  //     <div className="flex min-h-[calc(100vh-4rem)] items-center justify-center p-6">
  //       <Card className="max-w-md">
  //         <CardHeader>
  //           <CardTitle>Не удалось загрузить подразделения</CardTitle>
  //           <CardDescription>{error.message}</CardDescription>
  //           <Button type="button"
  //           onClick={() => refetch()}
  //           disabled={isFetching}
  //           className="mt-4">
  //             Повторить
  //           </Button>
  //         </CardHeader>
  //       </Card>
  //     </div>
  //   );
  // }

  // return (
  //   <main className="min-h-[calc(100vh-4rem)] bg-background text-foreground">
  //     <div className="mx-auto w-full max-w-7xl px-4 py-8 sm:px-6 sm:py-10 lg:px-8">
  //       <section className="flex flex-col gap-6 border-b border-border/70 pb-8 sm:flex-row sm:items-end sm:justify-between">
  //         <div>
  //           <div className="mb-4 flex size-11 items-center justify-center rounded-xl bg-violet-500/10 text-violet-400 ring-1 ring-violet-400/20">
  //             <Building2 className="size-5" />
  //           </div>
  //           <p className="text-sm font-medium text-muted-foreground">
  //             Корпоративный справочник
  //           </p>
  //           <h1 className="mt-1 text-3xl font-semibold tracking-tight sm:text-4xl">
  //             Departments
  //           </h1>
  //           <p className="mt-3 max-w-2xl text-sm leading-6 text-muted-foreground sm:text-base">
  //             Структура подразделений и команд организации.
  //           </p>
  //         </div>

  //         <Button onClick = {() => createDepartment()} size="lg" type="button" className="w-full sm:w-auto">
  //           <Plus data-icon="inline-start" />
  //           Добавить подразделение
  //         </Button>
  //       </section>

  //       <section
  //         aria-label="Сводка по подразделениям"
  //         className="mt-8 grid grid-cols-2 gap-px overflow-hidden rounded-xl bg-border/70 ring-1 ring-border/70 sm:grid-cols-3"
  //       >
  //         <div className="bg-card p-4 sm:p-5">
  //           <p className="text-xs font-medium text-muted-foreground">
  //             Всего подразделений
  //           </p>
  //           <p className="mt-2 text-2xl font-semibold tabular-nums">
  //             {totalCount}
  //           </p>
  //         </div>
  //         <div className="bg-card p-4 sm:p-5">
  //           <p className="text-xs font-medium text-muted-foreground">
  //             На странице
  //           </p>
  //           <p className="mt-2 text-2xl font-semibold tabular-nums">
  //             {departments.length}
  //           </p>
  //         </div>
  //         <div className="col-span-2 bg-card p-4 sm:col-span-1 sm:p-5">
  //           <p className="text-xs font-medium text-muted-foreground">
  //             Страница
  //           </p>
  //           <p className="mt-2 text-2xl font-semibold tabular-nums">
  //             {totalPages === 0 ? 0 : page}{" "}
  //             <span className="text-sm font-normal text-muted-foreground">
  //               из {totalPages}
  //             </span>
  //           </p>
  //         </div>
  //       </section>

  //       <section className="mt-8" aria-labelledby="departments-list-title">
  //         <div>
  //           <div>
  //             <h2 id="departments-list-title" className="text-lg font-semibold">
  //               Все подразделения
  //             </h2>
  //             <p className="mt-1 text-sm text-muted-foreground">
  //               Найдено: {totalCount}
  //             </p>
  //           </div>
  //         </div>

  //         {departments.length === 0 ? (
  //           <div className="mt-5 flex min-h-72 flex-col items-center justify-center rounded-xl border border-dashed border-border p-6 text-center">
  //             <FolderTree className="size-9 text-muted-foreground" />
  //             <p className="mt-4 font-medium">Подразделений пока нет</p>
  //             <p className="mt-1 max-w-md text-sm text-muted-foreground">
  //               Добавьте первое подразделение, чтобы оно появилось в справочнике.
  //             </p>
  //           </div>
  //         ) : (
  //           <div
  //             className={`mt-5 grid gap-4 md:grid-cols-2 xl:grid-cols-3 ${
  //               isFetching ? "opacity-70" : ""
  //             }`}
  //           >
  //             {departments.map((department) => {
  //               const pathParts = getPathParts(department.path);

  //               return (
  //                 <Card
  //                   key={department.id}
  //                   className="transition-colors hover:bg-muted/30"
  //                 >
  //                   <CardHeader>
  //                     <div className="mb-3 flex size-10 items-center justify-center rounded-lg bg-violet-500/10 text-violet-400">
  //                       <Building2 className="size-5" />
  //                     </div>
  //                     <CardTitle className="text-base">
  //                       {department.name}
  //                     </CardTitle>
  //                     <CardDescription className="font-mono text-xs">
  //                       {department.id}
  //                     </CardDescription>
  //                   </CardHeader>

  //                   <CardContent className="space-y-4">
  //                     <div className="flex items-start gap-2.5 text-sm">
  //                       <FolderTree className="mt-0.5 size-4 shrink-0 text-muted-foreground" />
  //                       <div className="flex min-w-0 flex-wrap items-center gap-1 text-muted-foreground">
  //                         {pathParts.length > 0 ? (
  //                           pathParts.map((part, index) => (
  //                             <span
  //                               key={`${part}-${index}`}
  //                               className="inline-flex items-center gap-1"
  //                             >
  //                               {index > 0 && (
  //                                 <ChevronRight className="size-3" />
  //                               )}
  //                               <span className="truncate">{part}</span>
  //                             </span>
  //                           ))
  //                         ) : (
  //                           <span>Корневое подразделение</span>
  //                         )}
  //                       </div>
  //                     </div>

  //                     <div className="flex items-center gap-2.5 border-t border-border/70 pt-4 text-xs text-muted-foreground">
  //                       <CalendarDays className="size-4" />
  //                       Создано{" "}
  //                       {new Intl.DateTimeFormat("ru-RU", {
  //                         dateStyle: "medium",
  //                       }).format(new Date(department.createdAt))}
  //                     </div>
  //                   </CardContent>
  //                 </Card>
  //               );
  //             })}
  //           </div>
  //         )}

  //         {totalPages > 1 && (
  //           <Pagination className="mt-8">
  //             <PaginationContent>
  //               <PaginationItem>
  //                 <PaginationPrevious
  //                   href={`?page=${page - 1}`}
  //                   text="Назад"
  //                   aria-disabled={page === 1}
  //                   className={
  //                     page === 1 ? "pointer-events-none opacity-50" : undefined
  //                   }
  //                   onClick={(event) => {
  //                     event.preventDefault();
  //                     setPage((currentPage) => Math.max(1, currentPage - 1));
  //                   }}
  //                 />
  //               </PaginationItem>

  //               {getVisiblePages(page, totalPages).map((item) => (
  //                 <PaginationItem key={item}>
  //                   {typeof item === "number" ? (
  //                     <PaginationLink
  //                       href={`?page=${item}`}
  //                       isActive={item === page}
  //                       onClick={(event) => {
  //                         event.preventDefault();
  //                         setPage(item);
  //                       }}
  //                     >
  //                       {item}
  //                     </PaginationLink>
  //                   ) : (
  //                     <PaginationEllipsis />
  //                   )}
  //                 </PaginationItem>
  //               ))}

  //               <PaginationItem>
  //                 <PaginationNext
  //                   href={`?page=${page + 1}`}
  //                   text="Вперёд"
  //                   aria-disabled={page === totalPages}
  //                   className={
  //                     page === totalPages
  //                       ? "pointer-events-none opacity-50"
  //                       : undefined
  //                   }
  //                   onClick={(event) => {
  //                     event.preventDefault();
  //                     setPage((currentPage) =>
  //                       Math.min(totalPages, currentPage + 1)
  //                     );
  //                   }}
  //                 />
  //               </PaginationItem>
  //             </PaginationContent>
  //           </Pagination>
  //         )}
  //       </section>
  //     </div>
  //   </main>
  // );
}
