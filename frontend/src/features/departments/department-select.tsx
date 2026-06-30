import { Checkbox } from "@/shared/components/ui/checkbox";
import { Label } from "@/shared/components/ui/label";
import { Spinner } from "@/shared/components/ui/spinner";
import { X } from "lucide-react";
import { useDepartmentsSelect } from "./model/use-departments-select";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/shared/components/ui/select";

type Base = { placeholder?: string; disabled?: boolean };
type Single = Base & {
  multiple?: false;
  value: string;
  onChange: (v: string) => void;
};
type Multi = Base & {
  multiple: true;
  value: string[];
  onChange: (v: string[]) => void;
};
type DepartmentSelectProps = Single | Multi;

export const NO_PARENT = "none";

export function DepartmentSelect(props: DepartmentSelectProps) {
  const { departments, isLoading, isFetchingNextPage, cursorRef } =
    useDepartmentsSelect();

  if (props.multiple) {
    const { value, onChange, disabled } = props;

    const toggle = (id: string) => {
      const next = value.includes(id)
        ? value.filter((d) => d !== id)
        : [...value, id];
      onChange(next);
    };

    return (
      <div className="grid gap-2">
        {value.length > 0 && (
          <div className="flex flex-wrap gap-1">
            {value.map((id) => {
              const dept = departments.find((d) => d.id === id);
              return (
                <button
                  key={id}
                  type="button"
                  disabled={disabled}
                  onClick={() => toggle(id)}
                  className="inline-flex items-center gap-1 rounded-full bg-secondary px-2 py-0.5 text-xs text-secondary-foreground transition-colors hover:bg-secondary/80 disabled:opacity-50"
                >
                  {dept?.name ?? id}
                  <X className="size-3" />
                </button>
              );
            })}
          </div>
        )}

        <div className="max-h-40 overflow-y-auto rounded-md border border-input p-3">
          {isLoading ? (
            <div className="flex justify-center py-2">
              <Spinner />
            </div>
          ) : (
            <div className="grid gap-2">
              {departments.map((dept) => (
                <div key={dept.id} className="flex items-center gap-2">
                  <Checkbox
                    id={`dept-${dept.id}`}
                    checked={value.includes(dept.id)}
                    disabled={disabled}
                    onCheckedChange={() => toggle(dept.id)}
                  />
                  <Label
                    htmlFor={`dept-${dept.id}`}
                    className="cursor-pointer font-normal"
                  >
                    {dept.name}
                  </Label>
                </div>
              ))}
              <div ref={cursorRef} className="flex justify-center py-1">
                {isFetchingNextPage && <Spinner />}
              </div>
            </div>
          )}
        </div>
      </div>
    );
  } else {
    const { value, onChange, placeholder, disabled } = props;

    return (
      <Select
        value={value}
        onValueChange={(id) => onChange(id)}
        disabled={disabled}
      >
        <SelectTrigger className="w-full">
          <SelectValue placeholder={placeholder} />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value={NO_PARENT}>{placeholder}</SelectItem>
          {isLoading ? (
            <div className="flex justify-center py-2">
              <Spinner />
            </div>
          ) : (
            departments.map((dept) => (
              <SelectItem key={dept.id} value={dept.id}>
                {dept.name}
              </SelectItem>
            ))
          )}
          <div ref={cursorRef} className="flex justify-center py-1">
            {isFetchingNextPage && <Spinner />}
          </div>
        </SelectContent>
      </Select>
    );
  }
}
