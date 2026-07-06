import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { setFilterDepartmentIds, setFilterStatus, useGetLocationFilter } from "./locations-filter-store";
import { useEffect } from "react";

export function useLocationsUrlFilter() {
  const params = useSearchParams();
  const router = useRouter();
  const pathname = usePathname();

  useEffect(() => {
    const raw = params.get("departmentIds");
    setFilterDepartmentIds(raw ? raw.split(",") : undefined);

    const status = params.get("status");
    setFilterStatus(status ?? "all");
  }, []);

  const { departmentIds, status } = useGetLocationFilter();

  useEffect(() => {
    const qs = new URLSearchParams(params);

    if (departmentIds?.length) {
      qs.set("departmentIds", departmentIds.join(","));
    } else {
      qs.delete("departmentIds");
    }

    if (status) {
      qs.set("status", status);
    } else {
      qs.delete("status");
    }

    const query = qs.toString();
    router.replace(query ? `${pathname}?${query}` : pathname);
  }, [departmentIds, status]);
}
