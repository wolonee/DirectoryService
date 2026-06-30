import { Checkbox } from "@/shared/components/ui/checkbox";
import { Label } from "@/shared/components/ui/label";
import { Spinner } from "@/shared/components/ui/spinner";
import { useDepartmentsSelect } from "./model/use-departments-select";

type Base = { placeholder?: string; disabled?: boolean };
type Single = Base & {
  multiple?: false;
  value: string | null;
  onChange: (v: string | null) => void;
};
type Multi = Base & {
  multiple: true;
  value: string[];
  onChange: (v: string[]) => void;
};
type DepartmentSelectProps = Single | Multi;

export function DepartmentSelect(props: DepartmentSelectProps) {
  // мозг: данные одинаковы для обоих вариантов. Хук зовём безусловно (правило хуков).
  const { departments, isLoading, isFetchingNextPage, cursorRef } =
    useDepartmentsSelect();

  if (props.multiple) {
    // тут TS уже знает: props это Multi → value это string[]
    const { value, onChange, disabled } = props;

    const toggle = (id: string) => {
      const next = value.includes(id)
        ? value.filter((d) => d !== id) // был выбран → убираем
        : [...value, id]; // не был → добавляем
      onChange(next); // сообщаем наружу новый массив
    };

    return (
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
    );
  }

  // single — заглушка, сделаем следующим шагом (Шаг 4)
  return null;
}
