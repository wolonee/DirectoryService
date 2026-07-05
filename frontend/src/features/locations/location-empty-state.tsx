import { AddLocationDialog } from "@/features/locations/create-location-dialog";
import { MapPin, SearchX } from "lucide-react";
import { resetLocationFilter, useGetLocationFilter } from "./model/locations-filter-store";
import { Button } from "@/shared/components/ui/button";

export default function LocationEmptyState() {
  const { departmentIds, search, isActive } = useGetLocationFilter();

  const isSearching = !!search?.trim();
  const hasFilter = !!departmentIds?.length || isActive !== undefined || isSearching;

  // Есть активный фильтр, но результат пуст → понятный текст + сброс
  if (hasFilter) {
    return (
      <div className="flex items-center justify-center py-16">
        <div className="flex max-w-sm flex-col items-center gap-3 text-center">
          <SearchX className="size-8 text-muted-foreground" />
          <p className="font-medium">
            {isSearching
              ? `По запросу «${search}» ничего не найдено`
              : "Ничего не найдено по выбранным фильтрам"}
          </p>
          <p className="text-sm text-muted-foreground">
            Измените условия или сбросьте фильтр, чтобы увидеть все локации.
          </p>
          <Button variant="outline" size="sm" onClick={() => resetLocationFilter()}>
            Сбросить фильтр
          </Button>
        </div>
      </div>
    );
  }

  // Фильтров нет и локаций нет → база действительно пустая
  return (
    <div className="flex items-center justify-center py-16">
      <div className="flex flex-col items-center gap-3 text-center">
        <MapPin className="size-8 text-muted-foreground" />
        <p className="font-medium">Локаций пока нет</p>
        <AddLocationDialog />
      </div>
    </div>
  );
}
