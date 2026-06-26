import { z } from "zod";

export const locationSchema = z.object({
  name: z
    .string()
    .trim()
    .min(3, "Название должно содержать минимум 3 символа")
    .max(120, "Название должно содержать максимум 120 символов"),
  country: z.string().trim().min(1, "Укажите страну"),
  city: z.string().trim().min(1, "Укажите город"),
  street: z.string().trim().min(1, "Укажите улицу и дом"),
  timezone: z
    .string()
    .trim()
    .min(1, "Укажите часовой пояс")
    .regex(
      /^[A-Za-z_]+(?:\/[A-Za-z0-9_+-]+)+$/,
      "Используйте формат Europe/Moscow",
    ),
});

export type LocationFormData = z.infer<typeof locationSchema>;
