# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

Scope: the **frontend** (Next.js 16 / React 19, Feature-Sliced Design). Run every command below from `frontend/`.

@AGENTS.md

> ⚠️ Next.js 16 has breaking changes vs. earlier versions — `AGENTS.md` (imported above) requires reading the bundled guide in `node_modules/next/dist/docs/` before writing routing/server-component code, and defines the commit-message convention. Read it.

## Commands

- **Dev server:** `npm run dev` (expects the API at `http://localhost:5057/api`)
- **Build:** `npm run build` · **Lint:** `npm run lint` · **Typecheck:** `npx tsc --noEmit`

Install frontend packages **from inside `frontend/`**. There are `node_modules` + `package.json` at both the repo root and here; a dependency installed at the root will not resolve in the Next.js build.

## Architecture

**Feature-Sliced Design** with strict downward imports `app → features → entities → shared`. Slices on the **same** layer must not import each other, and layers are global — never nest a layer folder (e.g. `features/`) inside a slice. A slice is organized into **segments** (`ui/`, `model/`, `api.ts`, `types.ts`) and exposes a public API via its `index.ts`.

- `app/` — Next.js routing. Keep pages as **thin server components** that render a feature (e.g. `page.tsx` → `<DepartmentsList />`); push `"use client"` and hooks down into `features/`.
- `entities/<domain>/` — one slice per backend domain (locations, departments, positions): `api.ts` (axios calls + TanStack `queryOptions`/`infiniteQueryOptions`), `types.ts` (mirror the C# contracts), `model/` (entity hooks like `use-department-by-id`), `ui/` (reusable domain components, e.g. a department select).
- `features/<domain>/` — dialogs, forms, lists, and their `model/` hooks/stores. A reusable domain widget needed by several features belongs **below** them (in the relevant `entities/<domain>/ui/`), not in another feature.
- `shared/` — UI kit (`components/ui`, shadcn via the `radix-ui` umbrella package), axios instance, cross-cutting hooks.

**Server state = TanStack Query.** Each entity exposes `queryOptions`/`infiniteQueryOptions` (the reusable "recipe" — a shared `queryKey` + `queryFn`); consume infinite options with `useInfiniteQuery`, single ones with `useQuery`. Define the `queryKey` once in the options object so hooks, invalidation, and prefetch all agree. Lists use `useInfiniteQuery` with a `select` that flattens pages, de-dupes by id, and returns a `PaginationResponse<T>` shape; scroll loading via a `cursorRef` (`useIntersectionRef`). Toggle lazy queries with `enabled` (call the hook unconditionally — Rules of Hooks — and gate the fetch). `QueryClient` uses `staleTime: 5min`, `refetchOnWindowFocus: false`. **Critical rule:** every value used inside `queryFn` must also appear in `queryKey`, or the query won't refetch when it changes.

**UI/filter state = Zustand** (per-feature stores, e.g. `org-structure-filter-store.ts`), exposed via a `useGet…()` selector. A selector that returns a **new object** must be wrapped in `useShallow` (`zustand/react/shallow`) — otherwise it compares by reference and causes an infinite render loop. Update state immutably (`[...arr, x]`, new object) so subscribers actually re-render. **Forms = React Hook Form + Zod** (`zodResolver`); toasts via `sonner`.

**Talking to the API:** query-string param keys must match the backend C# request-record property names, which are **PascalCase** — e.g. `Search`, `IsActive`, `SortBy`, `SortDir`, and nested `Pagination.Page` / `Pagination.PageSize`. The axios baseURL already includes `/api`. Responses are wrapped in `Envelope<T>` (`{ result, errors }`); paged results carry `{ items, totalCount, page, pageSize, totalPages }` — read `page`/`pageSize`/`totalPages` from the backend rather than recomputing them.

**Trees / hierarchical data.** Render recursively (a node component that renders itself for its children) rather than hand-nesting JSX; indent by `depth`. Store children per-parent (the Query cache keyed by parent id serves as `childrenByParentId`) and fetch a branch lazily on expand — do not rebuild the whole tree each render.
