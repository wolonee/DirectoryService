import { GetPositionDto } from "@/entities/positions/types";

type Props = {
  positions: GetPositionDto[];
  totalCount: number | undefined;
};

export default function PositionStats({ positions, totalCount }: Props) {
  return (
    <section
      aria-label="Сводка по должностям"
      className="mt-8 grid grid-cols-2 gap-px overflow-hidden rounded-xl bg-border/70 ring-1 ring-border/70 sm:grid-cols-3">
      <div className="bg-card p-4 sm:p-5">
        <p className="text-xs font-medium text-muted-foreground">
          Всего должностей
        </p>
        <p className="mt-2 text-2xl font-semibold tabular-nums">{totalCount}</p>
      </div>
      <div className="bg-card p-4 sm:p-5">
        <p className="text-xs font-medium text-muted-foreground">На странице</p>
        <p className="mt-2 text-2xl font-semibold tabular-nums">
          {positions.length}
        </p>
      </div>
      <div className="bg-card p-4 sm:p-5">
        <p className="text-xs font-medium text-muted-foreground">Направлений</p>
        <p className="mt-2 text-2xl font-semibold tabular-nums">
          {new Set(positions.map((p) => p.direction)).size}
        </p>
      </div>
    </section>
  );
}
