import { apiClient } from '@/shared/services/apiClient';

/**
 * Feedback Service — UC-09: Academic Performance Feedback
 */
export const feedbackService = {
  /**
   * Lấy danh sách HS + trạng thái đánh giá cho 1 session.
   * @param {number} sessionId
   */
  getFeedbackList: (sessionId) =>
    apiClient.get(`/teacher/sessions/${sessionId}/feedbacks`),

  /**
   * Lấy feedback detail cho 1 student trong 1 session.
   * @param {number} sessionId
   * @param {number} studentId
   */
  getStudentFeedback: (sessionId, studentId) =>
    apiClient.get(`/teacher/sessions/${sessionId}/feedbacks/${studentId}`),

  /**
   * Submit feedback cho 1 student.
   * @param {number} sessionId
   * @param {{ studentId: number, score: number, content: string }} data
   */
  submitFeedback: (sessionId, data) =>
    apiClient.post(`/teacher/sessions/${sessionId}/feedbacks`, data),
};
