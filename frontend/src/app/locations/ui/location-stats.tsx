import { GetLocationDto } from "@/entities/locations/types";

type Props = {
  locations: GetLocationDto[] | undefined;
  totalCount: number | undefined;
};

export default function LocationStats({ locations, totalCount }: Props) {
  const departmentsCount = 0;
  const timezonesCount = new Set(
    locations?.map((location) => location.timezone),
  ).size;

  return (
    <section
      aria-label="Сводка по локациям"
      className="grid grid-cols-2 gap-px overflow-hidden rounded-xl bg-border/70 ring-1 ring-border/70 sm:grid-cols-4"
    >
      <div className="bg-card p-4 sm:p-5">
        <p className="text-xs font-medium text-muted-foreground">
          Всего локаций
        </p>
        <p className="mt-2 text-2xl font-semibold tabular-nums">{totalCount}</p>
      </div>
      <div className="bg-card p-4 sm:p-5">
        <p className="text-xs font-medium text-muted-foreground">На странице</p>
        <p className="mt-2 text-2xl font-semibold tabular-nums">
          {locations?.length}
        </p>
      </div>
      <div className="bg-card p-4 sm:p-5">
        <p className="text-xs font-medium text-muted-foreground">
          Подразделений
        </p>
        <p className="mt-2 text-2xl font-semibold tabular-nums">
          {departmentsCount}
        </p>
      </div>
      <div className="bg-card p-4 sm:p-5">
        <p className="text-xs font-medium text-muted-foreground">
          Часовых поясов
        </p>
        <p className="mt-2 text-2xl font-semibold tabular-nums">
          {timezonesCount}
        </p>
      </div>
    </section>
  );
}
