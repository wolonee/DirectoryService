import { AddLocationDialog } from "@/features/locations/create-location-dialog";
import { MapPin } from "lucide-react";



export default function LocationEmptyState() {
    return (
      <div className="flex min-h-[calc(100vh-4rem)] items-center justify-center p-6">
        <div className="flex flex-col items-center gap-3 text-center">
          <MapPin className="size-8 text-muted-foreground" />
          <div>
            <p className="font-medium">Локаций пока нет</p>
          </div>
          <AddLocationDialog/>
        </div>
      </div>
    );
}