import { AddLocationDialog } from "@/features/locations/create-location-dialog";
import { MapPin, SearchX } from "lucide-react";

type Props = {
  search?: string;
};

export default function LocationEmptyState({ search }: Props) {
  const isSearching = !!search?.trim();

  return (
    <div className="flex items-center justify-center py-16">
      <div className="flex flex-col items-center gap-3 text-center">
        {isSearching ? (
          <>
            <SearchX className="size-8 text-muted-foreground" />
            <p className="font-medium">
              По запросу «{search}» ничего не найдено
            </p>
          </>
        ) : (
          <>
            <MapPin className="size-8 text-muted-foreground" />
            <p className="font-medium">Локаций пока нет</p>
            <AddLocationDialog />
          </>
        )}
      </div>
    </div>
  );
}
