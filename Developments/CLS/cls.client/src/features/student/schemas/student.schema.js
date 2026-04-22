import { z } from 'zod';

const phoneRegex = /^\d{10,11}$/;

/**
 * Schema tạo mới học sinh — khớp với CreateStudentRequest (Backend).
 * Validation messages bằng Tiếng Việt.
 */
export const createStudentSchema = z.object({
  // Học sinh
  fullName: z
    .string()
    .min(1, 'Họ và tên học sinh là bắt buộc.')
    .max(255, 'Họ và tên không được vượt quá 255 ký tự.'),
  dateOfBirth: z
    .string()
    .optional()
    .refine((val) => !val || new Date(val) < new Date(), {
      message: 'Ngày sinh phải là ngày trong quá khứ.',
    }),

  // Phụ huynh
  parentEmail: z
    .string()
    .min(1, 'Parent Email is strictly required to enable zero-touch automated notifications.')
    .email('Email phụ huynh không đúng định dạng.'),
  parentFullName: z
    .string()
    .min(1, 'Họ và tên phụ huynh là bắt buộc.')
    .max(255, 'Họ và tên phụ huynh không được vượt quá 255 ký tự.'),
  parentPhone: z
    .string()
    .optional()
    .refine((val) => !val || phoneRegex.test(val), {
      message: 'Số điện thoại phải có 10-11 chữ số.',
    }),
  parentRelationship: z
    .string()
    .min(1, 'Quan hệ với học sinh là bắt buộc.')
    .max(50, 'Quan hệ không được vượt quá 50 ký tự.'),
});

/**
 * Schema cập nhật thông tin học sinh — khớp với UpdateStudentRequest.
 */
export const updateStudentSchema = z.object({
  fullName: z
    .string()
    .min(1, 'Họ và tên học sinh là bắt buộc.')
    .max(255, 'Họ và tên không được vượt quá 255 ký tự.'),
  dateOfBirth: z
    .string()
    .optional()
    .refine((val) => !val || new Date(val) < new Date(), {
      message: 'Ngày sinh phải là ngày trong quá khứ.',
    }),
});
