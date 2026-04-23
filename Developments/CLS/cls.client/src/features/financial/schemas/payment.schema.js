import { z } from 'zod';

/**
 * Zod schema cho form ghi thanh toán mới.
 * Validation messages bằng Tiếng Việt.
 */
export const recordPaymentSchema = z.object({
  studentId: z
    .number({ required_error: 'ID học sinh là bắt buộc.' })
    .int()
    .positive('ID học sinh phải lớn hơn 0.'),

  tuitionPackageId: z
    .number({ required_error: 'Vui lòng chọn gói học.' })
    .int()
    .positive('Vui lòng chọn gói học.'),

  amountPaid: z
    .number({ required_error: 'Số tiền là bắt buộc.' })
    .min(0, 'Số tiền phải >= 0.'),
});
