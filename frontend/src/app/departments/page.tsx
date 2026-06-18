import { Building2 } from "lucide-react";
import type { Metadata } from "next";

import { DirectorySectionPlaceholder } from "@/components/directory-section-placeholder";

export const metadata: Metadata = {
  title: "Departments",
  description: "Структура подразделений и команд организации.",
};

export default function DepartmentsPage() {
  return (
    <DirectorySectionPlaceholder
      title="Departments"
      description="Здесь будет отображаться структура подразделений и команд."
      icon={Building2}
    />
  );
}
