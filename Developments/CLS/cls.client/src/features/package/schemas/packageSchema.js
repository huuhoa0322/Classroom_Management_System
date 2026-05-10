import { z } from 'zod';

export const packageSchema = z.object({
  name: z.string().min(1, 'Tên gói không được để trống.').max(100),
  totalSessions: z.coerce.number().int().min(1, 'Số buổi phải lớn hơn 0.'),
  durationDays: z.coerce.number().int().min(1, 'Thời hạn phải lớn hơn 0.'),
  price: z.coerce.number().min(1000, 'Giá gói phải ≥ 1,000 VNĐ.'),
});
