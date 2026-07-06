import { useState } from "react";
import { Input } from "@/shared/components/ui/input";
import { SortDirectionOptions } from "@/shared/common-constants";
import {
  Select,
  SelectTrigger,
  SelectValue,
  SelectContent,
  SelectItem,
} from "@/shared/components/ui/select";
import { Search, X } from "lucide-react";
import { resetLocationFilter, setFilterDepartmentIds, setFilterIsActive, setFilterSearch, setFilterSortBy, setFilterSortDirection, useGetLocationFilter } from "./model/locations-filter-store";
import { LocationSortByOptions } from "@/entities/locations/types";
import { DepartmentSelect } from "@/entities/departments/features/department-select";
import { Button } from "@/shared/components/ui/button";

export function LocationFilters() {
  const { departmentIds, search, isActive, sortBy, sortDirection } = useGetLocationFilter();

  // Только UI: держит выбор пункта «Архивные». Логику архива подключим позже.
  const [isArchived, setIsArchived] = useState(false);

  const hasFilter = !!departmentIds?.length || isActive !== undefined || !!search?.trim();

  return (
    <div className="mt-4 flex flex-col gap-3 sm:flex-row sm:items-center">

      <div className="relative w-full lg:max-w-xs">
        <Search className="absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
        <Input
          type="search"
          placeholder="Поиск локаций..."
          className="pl-9"
          value={search ?? ""}
          onChange={(e) => setFilterSearch(e.target.value)}
        />
      </div>

      <Select
        value={
          isArchived
            ? "archived"
            : isActive === undefined
              ? "all"
              : isActive
                ? "active"
                : "inactive"
        }
        onValueChange={(value: string) => {
          if (value === "archived") {
            // Только UI: логику архива пока не применяем.
            setIsArchived(true);
            return;
          }

          setIsArchived(false);
          if (value === "all") setFilterIsActive(undefined);
          else if (value === "active") setFilterIsActive(true);
          else setFilterIsActive(false);
        }}
      >
        <SelectTrigger className="w-full sm:w-40">
          <SelectValue placeholder="Статус" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="all">Все</SelectItem>
          <SelectItem value="active">Активные</SelectItem>
          <SelectItem value="inactive">Неактивные</SelectItem>
          <SelectItem value="archived">Архивные</SelectItem>
        </SelectContent>
      </Select>

      <Select value={sortBy} onValueChange={(value: string) => {
        if (value === "name") setFilterSortBy(LocationSortByOptions.name);
        else if (value === "country") setFilterSortBy(LocationSortByOptions.country);
        else setFilterSortBy(LocationSortByOptions.created_at);
      }}>
        <SelectTrigger className="w-full sm:w-40">
          <SelectValue placeholder="Сортировать по" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="name">Название</SelectItem>
          <SelectItem value="country">Страна</SelectItem>
          <SelectItem value="created_at">Дата создания</SelectItem>
        </SelectContent>
      </Select>

      <Select value={sortDirection} onValueChange={(value: string) => {
        if (value === "asc") setFilterSortDirection(SortDirectionOptions.asc)
        else setFilterSortDirection(SortDirectionOptions.desc)
      }}>
        <SelectTrigger className="w-full sm:w-44">
          <SelectValue placeholder="Направление" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="asc">По возрастанию</SelectItem>
          <SelectItem value="desc">По убыванию</SelectItem>
        </SelectContent>
      </Select>

      <DepartmentSelect
        multiple
        value={departmentIds ?? []}
        onChange={(next) => setFilterDepartmentIds(next)}
        placeholder="Департаменты"
        className="w-full sm:w-56"
      />

      {hasFilter && (
        <Button
          type="button"
          variant="ghost"
          size="sm"
          onClick={() => resetLocationFilter()}
          className="shrink-0 text-muted-foreground"
        >
          <X className="size-4" />
          Сбросить
        </Button>
      )}
    </div>
  );
}
