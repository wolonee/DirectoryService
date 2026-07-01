import { Spinner } from "@/shared/components/ui/spinner";
import { useGetOrgStructureFilter } from "./model/org-structure-filter-store";
import { useDepartmentById } from "@/entities/departments/model/use-department-by-id";

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

export function PositionsPanel() {

  const { selectedId } = useGetOrgStructureFilter();

  const { department, isLoading, isError, refetch } =
    useDepartmentById(selectedId);

  return (
    <section className="flex min-h-0 flex-col rounded-lg border bg-card">
      <div className="flex items-center justify-between border-b px-4 py-2.5">
        <div>
          <div className="text-sm font-medium">
            {department?.name ?? "Выберите подразделение"}
          </div>
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
  );
}
