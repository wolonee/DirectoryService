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
import {
  setDepartmentFilterIsActive,
  setDepartmentFilterSearch,
  setDepartmentFilterSortBy,
  setDepartmentFilterSortDir,
  useGetDepartmentFilter,
} from "./model/departments-filter-store";
import { DepartmentSelect, NO_PARENT } from "@/features/department-select";

export function DepartmentFilters() {
  const { search, isActive, sortBy, sortDir } = useGetDepartmentFilter();

  const [parentId, setParentId] = useState(NO_PARENT);

  return (
    <div className="mt-4 flex flex-col gap-3 sm:flex-row sm:items-center">
      <div className="relative w-full lg:max-w-xs">
        <Search className="absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
        <Input
          type="search"
          placeholder="Поиск подразделений..."
          className="pl-9"
          value={search ?? ""}
          onChange={(e) => setDepartmentFilterSearch(e.target.value)}
        />
      </div>

      <Select
        value={isActive === undefined ? "all" : isActive ? "active" : "inactive"}
        onValueChange={(value) => {
          if (value === "all") setDepartmentFilterIsActive(undefined);
          else if (value === "active") setDepartmentFilterIsActive(true);
          else setDepartmentFilterIsActive(false);
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

      <Select
        value={sortBy}
        onValueChange={(value) =>
          setDepartmentFilterSortBy(value as "name" | "created_at")
        }
      >
        <SelectTrigger className="w-full sm:w-40">
          <SelectValue placeholder="Сортировать по" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="name">Название</SelectItem>
          <SelectItem value="created_at">Дата создания</SelectItem>
        </SelectContent>
      </Select>

      <Select
        value={sortDir}
        onValueChange={(value) =>
          setDepartmentFilterSortDir(value as "asc" | "desc")
        }
      >
        <SelectTrigger className="w-full sm:w-44">
          <SelectValue placeholder="Направление" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="asc">По возрастанию</SelectItem>
          <SelectItem value="desc">По убыванию</SelectItem>
        </SelectContent>
      </Select>

      <DepartmentSelect 
        value={parentId}
        onChange={(id) => setParentId(id)}
        placeholder="Все родители"
      />
    </div>
  );
}
