import { BriefcaseBusiness } from "lucide-react";
import { AddPositionDialog } from "./create-position-dialog";

export function PostitionEmptyState() {
    return (
      <div className="flex min-h-[calc(100vh-4rem)] items-center justify-center p-6">
        <div className="flex flex-col items-center gap-3 text-center">
          <BriefcaseBusiness className="size-8 text-muted-foreground" />
          <div>
            <p className="font-medium">Должностей пока нет</p>
          </div>
          <AddPositionDialog />
        </div>
      </div>
    );
}