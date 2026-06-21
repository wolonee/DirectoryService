export type VisiblePage =
  | number
  | "start-ellipsis"
  | "end-ellipsis";

export function getVisiblePages(
  page: number,
  totalPages: number
): VisiblePage[] {
  const pages: VisiblePage[] = [];

  for (let currentPage = 1; currentPage <= totalPages; currentPage++) {
    const isFirstOrLast = currentPage === 1 || currentPage === totalPages;
    const isNearCurrent = Math.abs(currentPage - page) <= 1;

    if (isFirstOrLast || isNearCurrent) {
      pages.push(currentPage);
    } else if (currentPage < page && !pages.includes("start-ellipsis")) {
      pages.push("start-ellipsis");
    } else if (currentPage > page && !pages.includes("end-ellipsis")) {
      pages.push("end-ellipsis");
    }
  }

  return pages;
}
