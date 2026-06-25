import { BriefcaseBusiness } from "lucide-react";
import type { Metadata } from "next";

import { DirectorySectionPlaceholder } from "@/shared/components/directory-section-placeholder";

export const metadata: Metadata = {
  title: "Positions",
  description: "Должности, роли и зоны ответственности в организации.",
};

export default function PositionsPage() {
  return (
    <DirectorySectionPlaceholder
      title="Positions"
      description="Здесь будет управление должностями, ролями и зонами ответственности."
      icon={BriefcaseBusiness}
    />
  );
}
