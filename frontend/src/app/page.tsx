import {
  ArrowRight,
  BriefcaseBusiness,
  Building2,
  MapPin,
} from "lucide-react";
import type { Metadata } from "next";
import Link from "next/link";

import {
  Card,
  CardAction,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card";
import { routes } from "@/shared/routes";

export const metadata: Metadata = {
  title: {
    absolute: "Directory Service",
  },
  description:
    "Корпоративный справочник офисов, подразделений и должностей организации.",
};

const directorySections = [
  {
    title: "Locations",
    description: "Офисы, филиалы и рабочие площадки организации.",
    href: routes.locations,
    icon: MapPin,
    iconClassName: "bg-sky-500/10 text-sky-400",
  },
  {
    title: "Departments",
    description: "Структура подразделений и команд компании.",
    href: routes.departments,
    icon: Building2,
    iconClassName: "bg-emerald-500/10 text-emerald-400",
  },
  {
    title: "Positions",
    description: "Должности, роли и зоны ответственности.",
    href: routes.positions,
    icon: BriefcaseBusiness,
    iconClassName: "bg-amber-500/10 text-amber-400",
  },
];

export default function Home() {
  return (
    <main className="min-h-screen bg-background text-foreground">
      <section className="mx-auto w-full max-w-6xl px-6 py-12 sm:py-16">
        <div className="max-w-2xl">
          <p className="text-sm font-medium text-muted-foreground">
            Корпоративный справочник
          </p>
          <h1 className="mt-2 text-4xl font-semibold tracking-normal sm:text-5xl">
            Выберите раздел
          </h1>
          <p className="mt-4 text-base leading-7 text-muted-foreground">
            Управляйте основными данными об офисах, подразделениях и должностях
            из единой точки.
          </p>
        </div>

        <div className="mt-10 grid gap-4 md:grid-cols-3">
          {directorySections.map((section) => {
            const Icon = section.icon;

            return (
              <Link
                key={section.href}
                href={section.href}
                className="group rounded-xl outline-none focus-visible:ring-3 focus-visible:ring-ring/50"
              >
                <Card className="h-full transition-colors group-hover:bg-muted/50">
                  <CardHeader className="gap-5">
                    <div
                      className={`flex size-11 items-center justify-center rounded-lg ${section.iconClassName}`}
                    >
                      <Icon className="size-5" />
                    </div>
                    <div className="space-y-1">
                      <CardTitle className="text-lg">{section.title}</CardTitle>
                      <CardDescription className="leading-6">
                        {section.description}
                      </CardDescription>
                    </div>
                    <CardAction className="self-end">
                      <ArrowRight className="size-5 text-muted-foreground transition-transform group-hover:translate-x-1 group-hover:text-foreground" />
                    </CardAction>
                  </CardHeader>
                </Card>
              </Link>
            );
          })}
        </div>
      </section>
    </main>
  );
}
