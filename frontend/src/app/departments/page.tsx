import { Suspense } from "react";
import DepartmentsList from "@/features/departments/departments-list";

export default function DepartmentsPage() {
  return (
    <Suspense>
      <DepartmentsList />
    </Suspense>
  );
}
