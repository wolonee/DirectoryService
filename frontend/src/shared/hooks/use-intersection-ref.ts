import { RefCallback, useCallback } from "react";

// shared/hooks/use-intersection-ref.ts
type Props = {
    hasNextPage: boolean;
    isFetchingNextPage: boolean;
    fetchNextPage: () => void;
}

export function useIntersectionRef({
  hasNextPage,
  isFetchingNextPage,
  fetchNextPage,
}: Props) {
  return useCallback<RefCallback<HTMLDivElement>>(
    (el) => {
      const observer = new IntersectionObserver(
        (entries) => {
          if (entries[0].isIntersecting && hasNextPage && !isFetchingNextPage) {
            fetchNextPage();
          }
        },
        { threshold: 0.5 }
      );

      if (el) {
        observer.observe(el);
        return () => observer.disconnect();
      }
    },
    [fetchNextPage, hasNextPage, isFetchingNextPage]
  );
}
