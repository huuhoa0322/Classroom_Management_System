import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { feedbackService } from '../services/feedbackService';

/**
 * Hook: Lấy danh sách feedback cho 1 session.
 * @param {number|string} sessionId
 */
export function useFeedbackList(sessionId) {
  return useQuery({
    queryKey: ['feedback-list', sessionId],
    queryFn: () => feedbackService.getFeedbackList(sessionId),
    enabled: !!sessionId,
  });
}

/**
 * Hook: Lấy feedback detail cho 1 student trong 1 session.
 * @param {number|string} sessionId
 * @param {number|string} studentId
 */
export function useStudentFeedback(sessionId, studentId) {
  return useQuery({
    queryKey: ['student-feedback', sessionId, studentId],
    queryFn: () => feedbackService.getStudentFeedback(sessionId, studentId),
    enabled: !!sessionId && !!studentId,
  });
}

/**
 * Hook: Submit feedback cho 1 student.
 */
export function useSubmitFeedback() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ sessionId, data }) =>
      feedbackService.submitFeedback(sessionId, data),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: ['feedback-list', variables.sessionId],
      });
      queryClient.invalidateQueries({
        queryKey: ['student-feedback', variables.sessionId],
      });
    },
  });
}
