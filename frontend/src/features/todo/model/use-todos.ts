import { useState } from "react";

type Todo = {
  id: string;
  text: string;
  completed: boolean;
};

const initialTodos = [
        { id: "1", text: "readdd", completed: false },
        { id: "2", text: "writee", completed: true },
        { id: "3", text: "cool", completed: false },
    ]

export function useTodos() {
  const [todos, setTodos] = useState<Todo[]>(initialTodos);

  const addTodo = (text: string) => {
    const newTodo: Todo = {
      id: crypto.randomUUID(),
      text,
      completed: false,
    };

    setTodos((previousTodos) => [...previousTodos, newTodo]);
  };

  return { todos, setTodos, addTodo };
}