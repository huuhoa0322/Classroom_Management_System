import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getRooms, createRoom, updateRoom, updateRoomStatus } from '../services/roomService';

export const roomKeys = {
  all: ['rooms'],
  list: (params) => ['rooms', 'list', params],
};

export function useRoomList(page = 1, pageSize = 10) {
  return useQuery({ queryKey: roomKeys.list({ page, pageSize }), queryFn: () => getRooms({ page, pageSize }) });
}

export function useCreateRoom() {
  const qc = useQueryClient();
  return useMutation({ mutationFn: createRoom, onSuccess: () => qc.invalidateQueries({ queryKey: roomKeys.all }) });
}

export function useUpdateRoom() {
  const qc = useQueryClient();
  return useMutation({ mutationFn: ({ id, ...data }) => updateRoom(id, data), onSuccess: () => qc.invalidateQueries({ queryKey: roomKeys.all }) });
}

export function useUpdateRoomStatus() {
  const qc = useQueryClient();
  return useMutation({ mutationFn: ({ id, status }) => updateRoomStatus(id, { status }), onSuccess: () => qc.invalidateQueries({ queryKey: roomKeys.all }) });
}
