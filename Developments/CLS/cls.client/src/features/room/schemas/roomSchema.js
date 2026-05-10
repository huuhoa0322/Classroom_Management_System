import { z } from 'zod';

export const roomSchema = z.object({
  name: z.string().min(1, 'Tên phòng không được để trống.').max(100, 'Tên phòng không vượt quá 100 ký tự.'),
  capacity: z.coerce.number().int().min(1, 'Sức chứa phải lớn hơn 0.'),
});
