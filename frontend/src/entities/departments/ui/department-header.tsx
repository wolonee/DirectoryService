import { AddDepartmentDialog } from "@/features/departments/create-department-dialog";
import { Building2 } from "lucide-react";

export function DepartmentHeader() {
    return (
        <section className="flex flex-col gap-6 border-b border-border/70 pb-8 sm:flex-row sm:items-end sm:justify-between">
            <div>
            <div className="mb-4 flex size-11 items-center justify-center rounded-xl bg-violet-500/10 text-violet-400 ring-1 ring-violet-400/20">
                <Building2 className="size-5" />
            </div>
            <p className="text-sm font-medium text-muted-foreground">
                Корпоративный справочник
            </p>
            <h1 className="mt-1 text-3xl font-semibold tracking-tight sm:text-4xl">
                Departments
            </h1>
            <p className="mt-3 max-w-2xl text-sm leading-6 text-muted-foreground sm:text-base">
                Структура подразделений и команд организации.
            </p>
            </div>

            <AddDepartmentDialog />
        </section>
    )
}