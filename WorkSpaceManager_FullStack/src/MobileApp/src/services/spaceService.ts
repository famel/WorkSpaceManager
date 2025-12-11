import apiClient, { ApiResponse, PagedResponse } from './apiClient';

// Space types
export interface Desk {
  id: string;
  floorId: string;
  floorName?: string;
  buildingName?: string;
  deskNumber: string;
  hasMonitor: boolean;
  hasDockingStation: boolean;
  hasWindow: boolean;
  isAccessible: boolean;
  isAvailable: boolean;
}

export interface MeetingRoom {
  id: string;
  floorId: string;
  floorName?: string;
  buildingName?: string;
  name: string;
  roomNumber: string;
  capacity: number;
  hasProjector: boolean;
  hasWhiteboard: boolean;
  hasVideoConference: boolean;
  hasTelephone: boolean;
  isAccessible: boolean;
  equipment?: string;
  isAvailable: boolean;
}

export interface SearchDesksRequest {
  buildingId?: string;
  floorId?: string;
  hasMonitor?: boolean;
  hasDockingStation?: boolean;
  hasWindow?: boolean;
  isAccessible?: boolean;
  isAvailable?: boolean;
  pageNumber?: number;
  pageSize?: number;
}

export interface SearchMeetingRoomsRequest {
  buildingId?: string;
  floorId?: string;
  minCapacity?: number;
  hasProjector?: boolean;
  hasWhiteboard?: boolean;
  hasVideoConference?: boolean;
  isAccessible?: boolean;
  isAvailable?: boolean;
  pageNumber?: number;
  pageSize?: number;
}

class SpaceService {
  async searchDesks(request: SearchDesksRequest): Promise<ApiResponse<PagedResponse<Desk>>> {
    const response = await apiClient.post<ApiResponse<PagedResponse<Desk>>>(
      '/api/desks/search',
      request
    );
    return response.data;
  }

  async searchMeetingRooms(
    request: SearchMeetingRoomsRequest
  ): Promise<ApiResponse<PagedResponse<MeetingRoom>>> {
    const response = await apiClient.post<ApiResponse<PagedResponse<MeetingRoom>>>(
      '/api/meetingrooms/search',
      request
    );
    return response.data;
  }

  async getDesk(id: string): Promise<ApiResponse<Desk>> {
    const response = await apiClient.get<ApiResponse<Desk>>(`/api/desks/${id}`);
    return response.data;
  }

  async getMeetingRoom(id: string): Promise<ApiResponse<MeetingRoom>> {
    const response = await apiClient.get<ApiResponse<MeetingRoom>>(`/api/meetingrooms/${id}`);
    return response.data;
  }
}

export default new SpaceService();
