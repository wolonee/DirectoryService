import { GetDepartmentDto } from "@/entities/departments/types";
import {
  Card,
  CardAction,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card";
import { Building2, CalendarDays, ChevronRight, FolderTree } from "lucide-react";
import { UpdateDepartmentDialog } from "./update-department-dialog";
import { DeleteDepartmentDialog } from "./delete-department-dialog";

type Props = {
  department: GetDepartmentDto;
};

function getPathParts(path: string) {
  return path.split(/[./\\>]+/).filter(Boolean);
}

export default function DepartmentCard({ department }: Props) {
  const pathParts = getPathParts(department.path);

  return (
    <Card className="transition-colors hover:bg-muted/30">
      <CardHeader>
        <div className="mb-3 flex size-10 items-center justify-center rounded-lg bg-violet-500/10 text-violet-400">
          <Building2 className="size-5" />
        </div>
        <CardTitle className="text-base">{department.name}</CardTitle>
        <CardDescription className="font-mono text-xs">
          {department.id}
        </CardDescription>

        <CardAction className="flex items-center gap-1">
          <UpdateDepartmentDialog department={department} />
          <DeleteDepartmentDialog department={department} />
        </CardAction>
      </CardHeader>

      <CardContent className="space-y-4">
        <div className="flex items-start gap-2.5 text-sm">
          <FolderTree className="mt-0.5 size-4 shrink-0 text-muted-foreground" />
          <div className="flex min-w-0 flex-wrap items-center gap-1 text-muted-foreground">
            {pathParts.length > 0 ? (
              pathParts.map((part, index) => (
                <span
                  key={`${part}-${index}`}
                  className="inline-flex items-center gap-1"
                >
                  {index > 0 && <ChevronRight className="size-3" />}
                  <span className="truncate">{part}</span>
                </span>
              ))
            ) : (
              <span>Корневое подразделение</span>
            )}
          </div>
        </div>

        <div className="flex items-center gap-2.5 border-t border-border/70 pt-4 text-xs text-muted-foreground">
          <CalendarDays className="size-4" />
          Создано{" "}
          {new Intl.DateTimeFormat("ru-RU", {
            dateStyle: "medium",
          }).format(new Date(department.createdAt))}
        </div>
      </CardContent>
    </Card>
  );
}
