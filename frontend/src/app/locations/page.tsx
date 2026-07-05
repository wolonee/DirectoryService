import { Suspense } from "react";
import LocationsList from "@/features/locations/locations-list";

export default function LocationsPage() {
  return (
    <Suspense>
      <LocationsList />
    </Suspense>
  );
}