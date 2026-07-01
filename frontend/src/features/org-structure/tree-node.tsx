import { GetDepartmentChildrenByParentDto } from "@/entities/departments/types";
import { useChildrenList } from "./model/use-children-list";
import {
  setOrgStructureFilterExpandedIds,
  setOrgStructureFilterSelectedId,
  useGetOrgStructureFilter,
} from "./model/org-structure-filter-store";
import { Building2, ChevronDown, ChevronRight } from "lucide-react";
import { Spinner } from "@/shared/components/ui/spinner";

export function TreeNode({ node }: { node: GetDepartmentChildrenByParentDto }) {
  const { expandedIds, selectedId } = useGetOrgStructureFilter();
  const isExpanded = expandedIds.includes(node.id);

  const { children, isLoading, isError, refetch } = useChildrenList(
    node.id,
    isExpanded,
  );

  const childIndent = 8 + (node.depth + 1) * 18;

  const onNodeClick = (id: string) => {
    setOrgStructureFilterSelectedId(id);
    toggleExpand(id);
  };

  const toggleExpand = (id: string) => {
    const next = expandedIds.includes(id)
      ? expandedIds.filter((d) => d !== id)
      : [...expandedIds, id];
    setOrgStructureFilterExpandedIds(next);
  };

  return (
    <div>
      <button
        type="button"
        style={{ paddingLeft: 8 + node.depth * 18 }}
        onClick={() => onNodeClick(node.id)}
        className={`flex w-full items-center gap-1.5 rounded-md py-1.5 pr-2 text-left text-sm transition-colors hover:bg-accent ${
          selectedId === node.id
            ? "bg-accent font-medium text-accent-foreground"
            : "text-foreground"
        }`}
      >
        {node.hasMoreChildren ? (
          expandedIds.includes(node.id) ? (
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

      {isExpanded && (
        <>
          {isLoading && (
            <div
              className="flex items-center gap-2 py-1.5 text-xs text-muted-foreground"
              style={{ paddingLeft: childIndent }}
            >
              <Spinner className="size-3" />
              Загрузка…
            </div>
          )}

          {isError && (
            <div
              className="flex items-center gap-2 py-1.5 text-xs text-destructive"
              style={{ paddingLeft: childIndent }}
            >
              <span>Не удалось загрузить</span>
              <button
                type="button"
                onClick={() => refetch()}
                className="rounded px-1.5 py-0.5 font-medium underline-offset-2 hover:underline"
              >
                Повторить
              </button>
            </div>
          )}

          {!isLoading &&
            !isError &&
            children.map((child) => <TreeNode key={child.id} node={child} />)}
        </>
      )}
    </div>
  );
}
