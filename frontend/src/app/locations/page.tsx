import { MapPin } from "lucide-react";
import type { Metadata } from "next";
import { DirectorySectionPlaceholder } from "@/components/directory-section-placeholder";

export const metadata: Metadata = {
  title: "Locations",
  description: "Управление офисами, филиалами и рабочими площадками организации.",
};

export default function LocationsPage() {
  return (
    <DirectorySectionPlaceholder
      title="Locations"
      description="Здесь будет управление офисами, филиалами и рабочими площадками."
      icon={MapPin}
    />
  );
}
