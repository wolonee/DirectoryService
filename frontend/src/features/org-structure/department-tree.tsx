import { useRootsList } from "./model/use-roots-list";
import { TreeNode } from "./tree-node";
import { Spinner } from "@/shared/components/ui/spinner";

export function DepartmentTree() {
  const { roots, isLoading, isError, refetch } = useRootsList();

  if (isLoading) {
    return (
      <div className="flex flex-1 items-center justify-center gap-2 p-4 text-sm text-muted-foreground">
        <Spinner className="size-4" />
        Загрузка подразделений…
      </div>
    );
  }

  if (isError) {
    return (
      <div className="flex flex-1 flex-col items-center justify-center gap-2 p-4 text-center text-sm">
        <span className="text-destructive">Не удалось загрузить подразделения</span>
        <button
          type="button"
          onClick={() => refetch()}
          className="rounded-md border px-3 py-1.5 text-xs font-medium transition-colors hover:bg-accent"
        >
          Повторить
        </button>
      </div>
    );
  }

  if (roots.length === 0) {
    return (
      <div className="flex flex-1 items-center justify-center p-4 text-sm text-muted-foreground">
        Подразделений пока нет
      </div>
    );
  }

  return (
    <div className="min-h-0 flex-1 overflow-y-auto p-1.5">
      {roots.map((root) => (
        <TreeNode key={root.id} node={root} />
      ))}
    </div>
  );
}
