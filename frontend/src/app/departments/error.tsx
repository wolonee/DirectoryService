"use client";

import { useEffect } from "react";
import { Button } from "@/shared/components/ui/button";
import {
  Card,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card";

export default function LocationsError({
  error,
  unstable_retry,
}: {
  error: Error & { digest?: string };
  unstable_retry: () => void;
}) {
  useEffect(() => {
    console.error(error);
  }, [error]);

  return (
    <div className="flex min-h-[calc(100vh-4rem)] items-center justify-center p-6">
      <Card className="w-full max-w-md">
        <CardHeader>
          <CardTitle>Что-то пошло не так</CardTitle>
          <CardDescription>
            Не удалось отобразить страницу департаментов.
          </CardDescription>
          <Button type="button" onClick={() => unstable_retry()} className="mt-4">
            Повторить
          </Button>
        </CardHeader>
      </Card>
    </div>
  );
}
