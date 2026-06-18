import { ArrowLeft, type LucideIcon } from "lucide-react";
import Link from "next/link";

import { routes } from "@/shared/routes";

type DirectorySectionPlaceholderProps = {
  title: string;
  description: string;
  icon: LucideIcon;
};

export function DirectorySectionPlaceholder({
  title,
  description,
  icon: Icon,
}: DirectorySectionPlaceholderProps) {
  return (
    <main className="min-h-screen bg-background px-6 py-10 text-foreground">
      <div className="mx-auto w-full max-w-5xl">
        <Link
          href={routes.home}
          className="inline-flex items-center gap-2 text-sm font-medium text-muted-foreground transition-colors hover:text-foreground"
        >
          <ArrowLeft className="size-4" />
          На главную
        </Link>

        <section className="mt-16 border-t pt-8">
          <div className="flex size-12 items-center justify-center rounded-lg bg-secondary">
            <Icon className="size-6" />
          </div>
          <h1 className="mt-6 text-4xl font-semibold tracking-normal">{title}</h1>
          <p className="mt-3 max-w-xl leading-7 text-muted-foreground">
            {description}
          </p>
        </section>
      </div>
    </main>
  );
}
