import { Building2, ChevronDown, ChevronRight } from "lucide-react";
import { Spinner } from "@/shared/components/ui/spinner";

// ⚠️ Всё ниже — только МОК для вёрстки. Ни хуков, ни запросов, ни выделенных
// компонентов. Реальные данные (root-уровень, дети по parentId, позиции)
// подключаются отдельно; дерево строится из childrenByParentId, а не из этого
// плоского массива — здесь массив взят лишь чтобы отрисовать пиксели.

type TreeRow = {
  id: string;
  name: string;
  depth: number;
  hasChildren: boolean;
  expanded: boolean;
  selected: boolean;
  loading?: boolean;
};

const treeRows: TreeRow[] = [
  { id: "dev", name: "Разработка", depth: 0, hasChildren: true, expanded: true, selected: false },
  { id: "backend", name: "Backend", depth: 1, hasChildren: true, expanded: true, selected: true },
  { id: "api", name: "API Team", depth: 2, hasChildren: false, expanded: false, selected: false },
  { id: "platform", name: "Platform", depth: 2, hasChildren: false, expanded: false, selected: false },
  { id: "frontend", name: "Frontend", depth: 1, hasChildren: true, expanded: false, selected: false },
  { id: "qa", name: "QA", depth: 1, hasChildren: true, expanded: true, selected: false, loading: true },
  { id: "marketing", name: "Маркетинг", depth: 0, hasChildren: true, expanded: false, selected: false },
  { id: "sales", name: "Продажи", depth: 0, hasChildren: false, expanded: false, selected: false },
];

type Position = {
  id: string;
  speciality: string;
  direction: string;
  description: string;
};

const positions: Position[] = [
  { id: "1", speciality: "Software Engineer", direction: "Backend", description: "Разработка API и сервисов" },
  { id: "2", speciality: "Senior Software Engineer", direction: "Backend", description: "Архитектура и ревью" },
  { id: "3", speciality: "Tech Lead", direction: "Backend", description: "Техническое лидерство команды" },
  { id: "4", speciality: "DevOps Engineer", direction: "Infrastructure", description: "CI/CD и инфраструктура" },
  { id: "5", speciality: "QA Engineer", direction: "Quality", description: "Автотесты и приёмка" },
];

const selectedName = "Backend";

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

          <div className="min-h-0 flex-1 overflow-y-auto p-1.5">
            {treeRows.map((node) => (
              <div key={node.id}>
                <button
                  type="button"
                  style={{ paddingLeft: 8 + node.depth * 18 }}
                  className={`flex w-full items-center gap-1.5 rounded-md py-1.5 pr-2 text-left text-sm transition-colors hover:bg-accent ${
                    node.selected
                      ? "bg-accent font-medium text-accent-foreground"
                      : "text-foreground"
                  }`}
                >
                  {/* треугольник раскрытия — только если есть дети */}
                  {node.hasChildren ? (
                    node.expanded ? (
                      <ChevronDown className="size-4 shrink-0 text-muted-foreground" />
                    ) : (
                      <ChevronRight className="size-4 shrink-0 text-muted-foreground" />
                    )
                  ) : (
                    <span className="size-4 shrink-0" /> // лист — место под иконку, но без кнопки
                  )}

                  <Building2 className="size-4 shrink-0 text-muted-foreground" />
                  <span className="truncate">{node.name}</span>
                </button>

                {/* состояние загрузки прямых детей раскрытого узла */}
                {node.loading && (
                  <div
                    className="flex items-center gap-2 py-1.5 text-xs text-muted-foreground"
                    style={{ paddingLeft: 8 + (node.depth + 1) * 18 }}
                  >
                    <Spinner className="size-3" />
                    Загрузка…
                  </div>
                )}
              </div>
            ))}
          </div>
        </aside>

        {/* ───────── Справа: позиции выбранного узла ───────── */}
        <section className="flex min-h-0 flex-col rounded-lg border bg-card">
          <div className="flex items-center justify-between border-b px-4 py-2.5">
            <div>
              <div className="text-sm font-medium">{selectedName}</div>
              <div className="text-xs text-muted-foreground">
                Позиции подразделения
              </div>
            </div>
            <span className="rounded-full bg-secondary px-2 py-0.5 text-xs text-secondary-foreground">
              {positions.length}
            </span>
          </div>

          <div className="min-h-0 flex-1 overflow-y-auto">
            <table className="w-full text-sm">
              <thead className="sticky top-0 bg-card text-left text-muted-foreground shadow-[inset_0_-1px_0_var(--border)]">
                <tr>
                  <th className="px-4 py-2 font-medium">Специальность</th>
                  <th className="px-4 py-2 font-medium">Направление</th>
                  <th className="px-4 py-2 font-medium">Описание</th>
                </tr>
              </thead>
              <tbody>
                {positions.map((position) => (
                  <tr
                    key={position.id}
                    className="border-b last:border-0 hover:bg-accent/40"
                  >
                    <td className="px-4 py-2.5 font-medium">
                      {position.speciality}
                    </td>
                    <td className="px-4 py-2.5 text-muted-foreground">
                      {position.direction}
                    </td>
                    <td className="px-4 py-2.5 text-muted-foreground">
                      {position.description}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>

            {/* сентинел подгрузки следующей порции позиций */}
            <div className="flex items-center justify-center gap-2 py-3 text-xs text-muted-foreground">
              <Spinner className="size-3" />
              Загрузка позиций…
            </div>
          </div>
        </section>
      </div>
    </main>
  );
}
