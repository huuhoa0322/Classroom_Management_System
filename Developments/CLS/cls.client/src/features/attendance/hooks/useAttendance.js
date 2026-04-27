import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getTimetable, getAttendanceSheet, submitAttendance } from '../services/attendanceService';

/**
 * Hook: Lấy lịch dạy của Teacher theo tuần.
 * @param {string} from - ISO date string (start of week)
 * @param {string} to - ISO date string (end of week)
 */
export function useTimetable(from, to) {
  return useQuery({
    queryKey: ['timetable', from, to],
    queryFn: () => getTimetable({ from, to }),
    enabled: !!from && !!to,
  });
}

/**
 * Hook: Lấy sheet điểm danh cho 1 buổi học.
 * @param {number|null} sessionId
 */
export function useAttendanceSheet(sessionId) {
  return useQuery({
    queryKey: ['attendanceSheet', sessionId],
    queryFn: () => getAttendanceSheet(sessionId),
    enabled: !!sessionId,
  });
}

/**
 * Hook: Submit điểm danh.
 * Invalidate cả timetable và attendanceSheet sau submit.
 */
export function useSubmitAttendance() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ sessionId, records }) => submitAttendance(sessionId, records),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['timetable'] });
      queryClient.invalidateQueries({ queryKey: ['attendanceSheet'] });
    },
  });
}
