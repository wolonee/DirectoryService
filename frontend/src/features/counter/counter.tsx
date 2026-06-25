"use client";

import { Button } from "@/shared/components/ui/button";
import useCounter from "@/features/counter/model/use-counter";



export default function Counter() {


  const { counter, handleClick } = useCounter();


  return (
    <div className="w-full max-w-sm rounded-lg border bg-card p-6 text-card-foreground shadow-sm">
      <div className="mb-5 space-y-1">
        <p className="text-sm font-medium text-muted-foreground">Counter</p>
        <CoolCount count={counter}/>
      </div>
      <Button className="w-full" onClick={() => handleClick()}>increase</Button>

      {counter >= 10 && <span>Pozdravliay!!!</span>}
    </div>
  );
}

type Props = {
  count: number;
};

function CoolCount({ count }: Props) {
  return <span className="text-4xl font-semibold tracking-normal">Count - {count}</span>;
}
