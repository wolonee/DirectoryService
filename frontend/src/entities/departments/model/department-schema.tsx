import z from "zod";

export const departmentSchema = z.object({
  name: z
    .string()
    .trim()
    .min(3, "Название должно содержать минимум 3 символа")
    .max(150, "Максимум 150 символов"),
  identifier: z
    .string()
    .trim()
    .min(1, "Укажите идентификатор")
    .max(150, "Максимум 150 символов"),
  locationIds: z.array(z.string()).min(1, "Выберите хотя бы одну локацию"),
  parentId: z.string(),
});