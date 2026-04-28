import { z } from 'zod';

/** Schema validation cho lớp học — dùng chung cho Create và Update. */
export const classSchema = z.object({
  name: z
    .string()
    .min(1, 'Tên lớp không được để trống.')
    .max(100, 'Tên lớp không được vượt quá 100 ký tự.'),
});
