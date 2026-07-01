import { Spinner } from "@/shared/components/ui/spinner";
import { useGetOrgStructureFilter } from "./model/org-structure-filter-store";
import { useDepartmentById } from "@/entities/departments/model/use-department-by-id";
import { usePositionsByDepartmentId } from "@/entities/departments/model/use-positions-by-department-id";

export function PositionsPanel() {
  const { selectedId } = useGetOrgStructureFilter();

  const { department } = useDepartmentById(selectedId);

  const {
    positions,
    isLoading: isLoadingPositions,
    isError: isErrorPositions,
    isFetchingNextPage,
    cursorRef,
    refetch: refetchPositions,
  } = usePositionsByDepartmentId(selectedId);

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
        {renderBody()}
      </div>
    </section>
  );

  function renderBody() {
    // подразделение ещё не выбрано
    if (selectedId == null) {
      return (
        <div className="flex h-full items-center justify-center p-4 text-sm text-muted-foreground">
          Выберите подразделение в дереве слева
        </div>
      );
    }

    // первичная загрузка позиций
    if (isLoadingPositions) {
      return (
        <div className="flex h-full items-center justify-center gap-2 p-4 text-sm text-muted-foreground">
          <Spinner className="size-4" />
          Загрузка позиций…
        </div>
      );
    }

    // ошибка загрузки
    if (isErrorPositions) {
      return (
        <div className="flex h-full flex-col items-center justify-center gap-2 p-4 text-center text-sm">
          <span className="text-destructive">Не удалось загрузить позиции</span>
          <button
            type="button"
            onClick={() => refetchPositions()}
            className="rounded-md border px-3 py-1.5 text-xs font-medium transition-colors hover:bg-accent"
          >
            Повторить
          </button>
        </div>
      );
    }

    // позиций нет
    if (positions.length === 0) {
      return (
        <div className="flex h-full items-center justify-center p-4 text-sm text-muted-foreground">
          У подразделения нет позиций
        </div>
      );
    }

    // список позиций
    return (
      <>
        <table className="w-full text-sm">
          <thead className="sticky top-0 bg-card text-left text-muted-foreground shadow-[inset_0_-1px_0_var(--border)]">
            <tr>
              <th className="px-4 py-2 font-medium">Специальность</th>
              <th className="px-4 py-2 font-medium">Направление</th>
              <th className="px-4 py-2 font-medium">Статус</th>
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
                  {position.isActive ? "Активна" : "Неактивна"}
                </td>
              </tr>
            ))}
          </tbody>
        </table>

        {/* сентинел подгрузки следующей порции позиций */}
        <div
          ref={cursorRef}
          className="flex items-center justify-center gap-2 py-3 text-xs text-muted-foreground"
        >
          {isFetchingNextPage && (
            <>
              <Spinner className="size-3" />
              Загрузка позиций…
            </>
          )}
        </div>
      </>
    );
  }
}
