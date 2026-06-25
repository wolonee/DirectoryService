import { GetPositionDto } from "@/entities/positions/types";
import { Button } from "@/shared/components/ui/button";
import {
  Card,
  CardHeader,
  CardTitle,
  CardDescription,
  CardAction,
  CardContent,
} from "@/shared/components/ui/card";
import { BriefcaseBusiness, Building2, Clock3 } from "lucide-react";

type Props = {
    position: GetPositionDto
}

export default function PositionCard({position} : Props) {
  return (
    <Card key={position.id} className="transition-colors hover:bg-muted/30">
      <CardHeader className="grid-cols-[auto_1fr_auto] items-start gap-x-4">
        <div
          className="relative flex size-11 items-center justify-center rounded-xl bg-violet-500/10 text-violet-400 ring-1 ring-violet-400/20"
          aria-hidden>
          <BriefcaseBusiness className="size-5" />
        </div>
        <div className="min-w-0">
          <CardTitle className="truncate text-base">
            {position.speciality}
          </CardTitle>
          <CardDescription className="mt-1">
            {position.direction}
          </CardDescription>
        </div>
        <CardAction className="static col-auto row-auto">
          <Button
            variant="ghost"
            size="icon"
            type="button"
            aria-label={`Действия для ${position.speciality}`}>
            <Building2 />
          </Button>
        </CardAction>
      </CardHeader>

      <CardContent>
        <div className="grid gap-3 text-sm text-muted-foreground sm:grid-cols-2">
          <div className="flex items-center gap-2.5">
            <Building2 className="size-4 shrink-0" />
            <span>{position.direction}</span>
          </div>
          <div className="flex items-center gap-2.5">
            <Clock3 className="size-4 shrink-0" />
            <span>
              {new Intl.DateTimeFormat("ru-RU", {
                dateStyle: "medium",
              }).format(new Date(position.createdAt))}
            </span>
          </div>
        </div>

        <div className="mt-5 flex items-center justify-between border-t border-border/70 pt-4">
          <span className="text-xs text-muted-foreground">
            Создана{" "}
            {new Intl.DateTimeFormat("ru-RU", {
              dateStyle: "medium",
            }).format(new Date(position.createdAt))}
          </span>
          <span className="inline-flex items-center rounded-full bg-violet-500/10 px-2.5 py-1 text-xs font-medium text-violet-400">
            {position.direction}
          </span>
        </div>
      </CardContent>
    </Card>
  );
}
