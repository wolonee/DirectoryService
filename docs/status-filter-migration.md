# Миграция фильтра `IsActive` → `Status` (для репликации на Departments)

Этот документ описывает, что было сделано для **Locations**, чтобы точно так же
повторить для **Departments**. Сделано в ветке `DS-F15`.

## Идея

Раньше список фильтровался булевым `IsActive`. Теперь — строковым `Status` с
4 значениями, который покрывает и активность, и архив (soft delete):

| status              | SQL-условие                                   |
| ------------------- | --------------------------------------------- |
| `all` (по умолчанию)| `is_deleted = false`                          |
| `active`            | `is_deleted = false AND is_active = true`     |
| `inactive`          | `is_deleted = false AND is_active = false`    |
| `archived`          | `is_deleted = true`                           |

Побочный бонус: раньше в запросе **не было** фильтра `is_deleted`, поэтому
soft-deleted записи протекали в обычный список. Теперь всё, кроме `archived`,
их исключает. У departments та же проблема — репликация её тоже чинит.

## ⚠️ Важно не спутать

`DepartmentDto.IsActive` (поле **ответа**, отдаёт флаг сущности клиенту) —
это НЕ фильтр. Его трогать не нужно. Меняется только **параметр фильтра**
`IsActive` в `GetDepartmentsRequest` и соответствующее условие в хендлере.

---

## Backend (Locations — что сделано)

1. **Контракт запроса** — `GetLocationsRequest.cs`
   `bool? IsActive` → `string? Status`.

2. **Хендлер** — `GetLocationsHandler.cs`
   - удалена константа `IS_ACTIVE_PARAMETER`;
   - блок `if (request.IsActive.HasValue) { ... l.is_active = @is_active ... }`
     заменён на `switch (request.Status?.ToLower())` по таблице выше
     (в `default` — `l.is_deleted = false`).

3. **Валидатор** — `GetLocationsValidator.cs`
   - добавлено поле `private static readonly string[] AllowedStatuses =
     ["all", "active", "inactive", "archived"];`
   - правило: `Status` (если не пуст) должен быть из `AllowedStatuses`,
     иначе `GeneralErrors.ValueIsInvalid(...)`.

### Backend (Departments — что повторить)

Файлы-аналоги:
- `src/DirectoryService.Contracts/Departments/Requests/GetDepartmentsRequest.cs`
  — заменить `bool? IsActive` на `string? Status`.
- `src/DirectoryService.Application/Departments/Queries/Get/GetDepartmentsHandler.cs`
  — `IS_ACTIVE_PARAMETER`/блок `IsActive` → `switch` по `Status`. Алиас таблицы
  у departments — `d`, то есть условия `d.is_deleted = ...`, `d.is_active = ...`.
- `src/DirectoryService.Application/Departments/Queries/Get/GetDepartmentsValidator.cs`
  — добавить `AllowedStatuses` и правило валидации (как в locations).

Проверить: `dotnet build` (0 errors; StyleCop-warnings — существующий baseline).
Миграция БД **не нужна** — колонки `is_active` / `is_deleted` уже есть.

---

## Frontend (Locations — что сделано)

FSD-слои. Пути указаны для locations; для departments — те же имена в
`features/departments` и `entities/departments`.

1. **Store** — `features/locations/model/locations-filter-store.ts`
   - тип: `isActive?: boolean` → `status: string` (обязательное, дефолт `"all"`);
   - `initialState.status = "all"`;
   - экшен `setIsActive` → `setStatus`; селектор `useGetLocationFilter` отдаёт `status`;
   - `resetLocationFilter` ставит `setStatus("all")`;
   - экспорт `setFilterIsActive` → `setFilterStatus`.

2. **Types** — `entities/locations/types.ts`
   - в `GetLocationsRequest`: `isActive?: boolean` → `status?: string`;
   - добавлен `LocationStatusOptions = { all, active, inactive, archived }`.

3. **API** — `entities/locations/api.ts`
   - params: `IsActive: request.isActive` → `Status: request.status`
     (ключ PascalCase — совпадает с C# `Status`);
   - в `getLocationsInfiniteOptions`: деструктуризация `isActive` → `status`,
     **и в `queryKey`, и в `queryFn`** (правило: значение из queryFn обязано
     быть в queryKey, иначе не рефетчит).

4. **List-хук** — `features/locations/model/use-locations-list.ts`
   - деструктурировать `status` и прокинуть в `getLocationsInfiniteOptions`.

5. **Страница/список** — `features/locations/locations-list.tsx`
   - взять `status` из `useGetLocationFilter` и передать в `useLocationsList`.

6. **Фильтры** — `features/locations/location-filters.tsx`
   - убран локальный `useState` статуса;
   - `value={status}` из store, `onValueChange={(v) => setFilterStatus(v)}`;
   - `hasFilter`: `isActive !== undefined` → `status !== "all"`;
   - в `<SelectContent>` пункт `<SelectItem value="archived">Архивные</SelectItem>`
     (Все / Активные / Неактивные / Архивные).

7. **Empty state** — `features/locations/location-empty-state.tsx`
   - `hasFilter`: `isActive !== undefined` → `status !== "all"`.

Проверить: `npx tsc --noEmit` (0 errors) и `npm run lint` (0 errors;
4 warning'а — существующий baseline не из этих файлов).

### Frontend (Departments — что повторить)

Аналогичные файлы:
- `features/departments/model/departments-filter-store.ts`
- `entities/departments/types.ts` (только **параметр запроса**, не поле DTO!)
- `entities/departments/api.ts`
- `features/departments/model/use-departments-list.ts`
- `features/departments/departments-list.tsx`
- `features/departments/department-filters.tsx`
- (если есть) departments empty-state

У departments поле сортировки называется `sortDir` (а не `sortDirection`) —
не переименовывать, менять только `isActive` → `status`.

---

## Вне scope (следующие шаги задачи «Архив», ещё не сделано)

- Сохранение `status` в URL (переживание F5).
- Поле `deletedAt` в DTO ответа + вывод даты удаления в архивном режиме.
- Кнопка «Восстановить», mutation-хук восстановления, confirm-диалог,
  инвалидация кэша активного и архивного списков.
