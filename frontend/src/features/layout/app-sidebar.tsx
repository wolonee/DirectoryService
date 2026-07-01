"use client";

import {
  BriefcaseBusiness,
  Building2,
  FlaskConical,
  FolderTree,
  MapPin,
  Network,
} from "lucide-react";
import Link from "next/link";
import { usePathname } from "next/navigation";

import { routes } from "@/shared/routes";
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarRail,
  useSidebar,
} from "@/shared/components/ui/sidebar";

const mainNavigation = [
  {
    title: "Locations",
    href: routes.locations,
    icon: MapPin,
  },
  {
    title: "Departments",
    href: routes.departments,
    icon: Building2,
  },
  {
    title: "Org structure",
    href: routes.orgStructure,
    icon: FolderTree,
  },
  {
    title: "Positions",
    href: routes.positions,
    icon: BriefcaseBusiness,
  },
];

const activeItemClassName =
  "data-[active=true]:bg-sidebar-primary data-[active=true]:text-sidebar-primary-foreground data-[active=true]:shadow-sm data-[active=true]:hover:bg-sidebar-primary data-[active=true]:hover:text-sidebar-primary-foreground data-[active=true]:[&_svg]:text-sidebar-primary-foreground";

export default function AppSidebar() {
  const pathname = usePathname();
  const isRouteActive = (href: string) =>
    pathname === href || pathname.startsWith(`${href}/`);

  const { setOpenMobile } = useSidebar();

  return (
    <Sidebar collapsible="icon">
      <SidebarHeader className="border-b border-sidebar-border">
        <SidebarMenu>
          <SidebarMenuItem>
            <SidebarMenuButton
              asChild
              size="lg"
              isActive={pathname === routes.home}
              className={activeItemClassName}
              tooltip="Directory Service"
              onClick={() => setOpenMobile(false)}
            >
              <Link
                href={routes.home}
                aria-current={pathname === routes.home ? "page" : undefined}
              >
                <span className="flex aspect-square size-8 items-center justify-center rounded-lg bg-sidebar-primary text-sidebar-primary-foreground">
                  <Network className="size-4" />
                </span>
                <span className="grid flex-1 text-left text-sm leading-tight">
                  <span className="truncate font-semibold">
                    Directory Service
                  </span>
                  <span className="truncate text-xs text-muted-foreground">
                    Organization
                  </span>
                </span>
              </Link>
            </SidebarMenuButton>
          </SidebarMenuItem>
        </SidebarMenu>
      </SidebarHeader>

      <SidebarContent>
        <SidebarGroup>
          <SidebarGroupLabel>Directory</SidebarGroupLabel>
          <SidebarGroupContent>
            <SidebarMenu>
              {mainNavigation.map((item) => {
                const isActive = isRouteActive(item.href);

                return (
                  <SidebarMenuItem key={item.href}>
                    <SidebarMenuButton
                      asChild
                      isActive={isActive}
                      className={activeItemClassName}
                      tooltip={item.title}
                      onClick={() => setOpenMobile(false)}
                    >
                      <Link
                        href={item.href}
                        aria-current={isActive ? "page" : undefined}
                      >
                        <item.icon />
                        <span>{item.title}</span>
                      </Link>
                    </SidebarMenuButton>
                  </SidebarMenuItem>
                );
              })}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      </SidebarContent>

      <SidebarFooter className="border-t border-sidebar-border">
        <SidebarMenu>
          <SidebarMenuItem>
            <SidebarMenuButton
              asChild
              isActive={isRouteActive(routes.playground)}
              className={activeItemClassName}
              tooltip="Playground"
              onClick={() => setOpenMobile(false)}
            >
              <Link
                href={routes.playground}
                aria-current={
                  isRouteActive(routes.playground) ? "page" : undefined
                }
              >
                <FlaskConical />
                <span>Playground</span>
              </Link>
            </SidebarMenuButton>
          </SidebarMenuItem>
        </SidebarMenu>
      </SidebarFooter>

      <SidebarRail />
    </Sidebar>
  );
}
