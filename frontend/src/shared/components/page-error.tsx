import { Button } from "./ui/button";
import { Card, CardDescription, CardHeader, CardTitle } from "./ui/card";

function getLocationsErrorMessage(error: Error) {
  if (error.message === "Network Error") {
    return "Не удалось подключиться к backend. Проверьте, что сервер запущен, и попробуйте снова.";
  }

  return error.message || "Произошла неизвестная ошибка.";
}

type Props = {
    refetch: () => void;
    isFetching: boolean;
    error: Error;
    name: string;
}

export default function PageError({refetch, isFetching, error, name} : Props) {
    return (
      <div className="flex min-h-[calc(100vh-4rem)] items-center justify-center p-6">
        <Card className="w-full max-w-lg">
          <CardHeader className="text-center">
            <CardTitle>Не удалось загрузить {name}</CardTitle>
            <CardDescription className="max-w-none whitespace-normal break-words leading-6">
              {getLocationsErrorMessage(error)}
            </CardDescription>
            <Button
              type="button"
              onClick={() => refetch()}
              disabled={isFetching}
              className="mx-auto mt-4 w-fit"
            >
              Повторить
            </Button>
          </CardHeader>
        </Card>
      </div>
    );
}