import { z } from 'zod';

export const createUserSchema = z.object({
  fullName: z.string().min(1, 'Họ tên không được để trống.').max(100),
  email: z.string().min(1, 'Email không được để trống.').email('Email không hợp lệ.'),
  phone: z.string().regex(/^\d{10,11}$/, 'SĐT phải có 10-11 chữ số.').or(z.literal('')).optional(),
  password: z.string().min(6, 'Mật khẩu phải có ít nhất 6 ký tự.'),
});

export const updateUserSchema = z.object({
  fullName: z.string().min(1, 'Họ tên không được để trống.').max(100),
  email: z.string().min(1, 'Email không được để trống.').email('Email không hợp lệ.'),
  phone: z.string().regex(/^\d{10,11}$/, 'SĐT phải có 10-11 chữ số.').or(z.literal('')).optional(),
});
