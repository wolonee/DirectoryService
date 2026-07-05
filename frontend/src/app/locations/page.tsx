import { Suspense } from "react";
import LocationsList from "@/features/locations/locations-list";

export default function LessonsPage() {
  return (
    <Suspense>
      <LocationsList />
    </Suspense>
  );
}