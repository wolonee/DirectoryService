"use client";

import { DepartmentTree } from "@/features/org-structure/department-tree";
import { PositionsPanel } from "@/features/org-structure/positions-panel";

export default function OrgStructurePage() {
  return (
    <main className="min-h-screen bg-background p-4 text-foreground sm:p-6">
      <div className="mb-4">
        <h1 className="text-2xl font-semibold">Оргструктура</h1>
        <p className="text-sm text-muted-foreground">
          Дерево подразделений и позиции выбранного узла.
        </p>
      </div>

      <div className="grid h-[calc(100vh-9rem)] grid-cols-1 gap-4 lg:grid-cols-[340px_1fr]">
        {/* ───────── Слева: дерево ───────── */}
        <aside className="flex min-h-0 flex-col rounded-lg border bg-card">
          <div className="border-b px-3 py-2.5 text-sm font-medium">
            Подразделения
          </div>

          <DepartmentTree />
        </aside>

        {/* ───────── Справа: позиции выбранного узла ───────── */}
        <PositionsPanel />
      </div>
    </main>
  );
}
