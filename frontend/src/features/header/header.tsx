import { Network } from "lucide-react";
import Link from "next/link";

import { routes } from "@/shared/routes";
import { SidebarTrigger } from "@/shared/components/ui/sidebar";

export default function Header() {
  return (
    <header className="sticky top-0 z-50 border-b border-border/70 bg-background/95 backdrop-blur">
      <div className="grid h-16 w-full grid-cols-[1fr_auto_1fr] items-center px-4 sm:px-6">
        <SidebarTrigger className="justify-self-start" />
        <Link
          href={routes.home}
          className="flex items-center gap-3 rounded-lg outline-none focus-visible:ring-3 focus-visible:ring-ring/50"
        >
          <span className="flex size-9 items-center justify-center rounded-lg bg-primary text-primary-foreground">
            <Network className="size-5" />
          </span>
          <span className="text-base font-semibold">Directory Service</span>
        </Link>
        <div aria-hidden />
      </div>
    </header>
  );
}
