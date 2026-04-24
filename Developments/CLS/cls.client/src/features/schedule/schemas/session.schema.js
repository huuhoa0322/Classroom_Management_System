import { z } from 'zod';

/**
 * Zod schema cho tạo/sửa buổi học (CLS-004).
 */
export const sessionSchema = z
  .object({
    classId: z.coerce.number().min(1, 'Vui lòng chọn lớp học.'),
    teacherId: z.coerce.number().min(1, 'Vui lòng chọn giáo viên.'),
    roomId: z.coerce.number().min(1, 'Vui lòng chọn phòng học.'),
    startTime: z.string().min(1, 'Vui lòng chọn thời gian bắt đầu.'),
    endTime: z.string().min(1, 'Vui lòng chọn thời gian kết thúc.'),
  })
  .refine((data) => new Date(data.endTime) > new Date(data.startTime), {
    message: 'Thời gian kết thúc phải sau thời gian bắt đầu.',
    path: ['endTime'],
  });
