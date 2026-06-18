"use client";

import { CheckCircle2, Circle, ListTodo, Trash2 } from "lucide-react";

import {
  Card,
  CardAction,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Checkbox } from "@/components/ui/checkbox";
import { useState } from "react";
import { Input } from "@/components/ui/input";
import { Button } from "@/ui/button";
import { useTodos } from "@/hooks/use-todos";


type Todo = {
  id: string;
  text: string;
  completed: boolean;
};

export default function Todo() {

    const [input, setInput] = useState("");

    const { todos, setTodos, addTodo } = useTodos();

    const handleAddTodo = (input: string) => {
        addTodo(input)
        setInput("")
    }

    const handleToggleTodo = (id: string) => {
        setTodos((prevTodos) => 
            prevTodos.map((todo) => {
                if (todo.id == id){
                    return { ...todo, completed: !todo.completed }
                }   
                return todo
            })
        );
    }

    const handleDeleteTask = (id: string) => {
        setTodos((prevTodos) =>
            prevTodos.filter((todo) => todo.id !== id)
        );
    }

    const handleCountTodos = () => {
        const uncompletedTasks = todos.filter((todo) => !todo.completed)
        return uncompletedTasks.length
    }

    const uncompletedTodosCount = handleCountTodos()

  return (
    <section className="mx-auto flex w-full max-w-2xl flex-col gap-4">
      <div className="space-y-1">
        <p className="text-sm font-medium text-muted-foreground">Playground</p>
        <h1 className="text-3xl font-semibold tracking-normal">Todo list</h1>
      </div>

      <Card size="sm">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <ListTodo className="size-4 text-muted-foreground" />
            Uncompleted tasks
          </CardTitle>
          <CardDescription>Tasks that still need your attention</CardDescription>
          <CardAction>
            <span className="flex size-9 items-center justify-center rounded-lg bg-secondary text-lg font-semibold text-secondary-foreground">
              {uncompletedTodosCount}
            </span>
          </CardAction>
        </CardHeader>
      </Card>

      <div className="">
        <Input placeholder="Task name" value={input} onChange={(event) => setInput(event.target.value)}/>
        <Button onClick={() => handleAddTodo(input)}>Add task</Button>
      </div>

      <div className="grid gap-3">
        {[...todos].reverse().map((todo) => (
          <Card
            key={todo.id}
            className={
              todo.completed ? "bg-card/70 text-muted-foreground" : undefined
            }
          >
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                {todo.completed ? (
                  <CheckCircle2 className="size-5 text-emerald-500" />
                ) : (
                  <Circle className="size-5 text-muted-foreground" />
                )}
                <span className={todo.completed ? "line-through" : undefined}>
                  {todo.text}
                </span>
              </CardTitle>
              <CardDescription>Task #{todo.id}</CardDescription>
              <CardAction>
                <div className="flex items-center gap-2">
                  <span
                    className={
                      todo.completed
                        ? "rounded-md bg-emerald-500/10 px-2 py-1 text-xs font-medium text-emerald-500"
                        : "rounded-md bg-secondary px-2 py-1 text-xs font-medium text-secondary-foreground"
                    }
                  >
                    {todo.completed ? "Done" : "Open"}
                  </span>
                  <Checkbox
                    checked={todo.completed}
                    onClick={() => handleToggleTodo(todo.id)}
                    aria-label={`Mark task ${todo.id} as completed`}
                  />
                  <Button
                    type="button"
                    onClick={() => handleDeleteTask(todo.id)}
                    variant="destructive"
                    size="icon-sm"
                    aria-label={`Delete task ${todo.text}`}
                    title="Delete task"
                  >
                    <Trash2 />
                  </Button>
                </div>
              </CardAction>
            </CardHeader>
            <CardContent>
              <p className="text-sm text-muted-foreground">
                {todo.completed
                  ? "This task is complete and ready to archive."
                  : "This task is still waiting for attention."}
              </p>
            </CardContent>
          </Card>
        ))}
      </div>
    </section>
  );
}
