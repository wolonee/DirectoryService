import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { useEffect } from "react";
import {
  setDepartmentFilterStatus,
  useGetDepartmentFilter,
} from "./departments-filter-store";

export function useDepartmentsUrlFilter() {
  const params = useSearchParams();
  const router = useRouter();
  const pathname = usePathname();

  useEffect(() => {
    const status = params.get("status");
    setDepartmentFilterStatus(status ?? "all");
  }, [params]);

  const { status } = useGetDepartmentFilter();

  useEffect(() => {
    const qs = new URLSearchParams(params);

    if (status) {
      qs.set("status", status);
    } else {
      qs.delete("status");
    }

    const query = qs.toString();
    const nextUrl = query ? `${pathname}?${query}` : pathname;
    const currentQuery = params.toString();
    const currentUrl = currentQuery ? `${pathname}?${currentQuery}` : pathname;

    if (nextUrl !== currentUrl) {
      router.replace(nextUrl);
    }
  }, [params, pathname, router, status]);
}
