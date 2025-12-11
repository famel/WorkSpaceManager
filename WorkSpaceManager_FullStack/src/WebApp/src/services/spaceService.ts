import apiClient, { ApiResponse, PagedResponse } from './api';

// Building types
export interface Building {
  id: string;
  name: string;
  address: string;
  totalFloors: number;
  isActive: boolean;
  floorsCount: number;
  totalDesks: number;
  totalMeetingRooms: number;
  createdAt: string;
}

export interface CreateBuildingRequest {
  name: string;
  address: string;
  totalFloors: number;
}

export interface UpdateBuildingRequest {
  name?: string;
  address?: string;
  totalFloors?: number;
  isActive?: boolean;
}

// Floor types
export interface Floor {
  id: string;
  buildingId: string;
  name: string;
  floorNumber: number;
  totalDesks: number;
  totalMeetingRooms: number;
  floorPlanUrl?: string;
  isActive: boolean;
  buildingName?: string;
  createdAt: string;
}

export interface CreateFloorRequest {
  buildingId: string;
  name: string;
  floorNumber: number;
  floorPlanUrl?: string;
}

export interface UpdateFloorRequest {
  name?: string;
  floorNumber?: number;
  floorPlanUrl?: string;
  isActive?: boolean;
}

// Desk types
export interface Desk {
  id: string;
  floorId: string;
  deskNumber: string;
  location?: string;
  hasMonitor: boolean;
  hasDockingStation: boolean;
  isNearWindow: boolean;
  isAccessible: boolean;
  isAvailable: boolean;
  notes?: string;
  floorName?: string;
  buildingName?: string;
  createdAt: string;
}

export interface CreateDeskRequest {
  floorId: string;
  deskNumber: string;
  location?: string;
  hasMonitor: boolean;
  hasDockingStation: boolean;
  isNearWindow: boolean;
  isAccessible: boolean;
  notes?: string;
}

export interface UpdateDeskRequest {
  deskNumber?: string;
  location?: string;
  hasMonitor?: boolean;
  hasDockingStation?: boolean;
  isNearWindow?: boolean;
  isAccessible?: boolean;
  isAvailable?: boolean;
  notes?: string;
}

export interface DeskSearchRequest {
  buildingId?: string;
  floorId?: string;
  isAvailable?: boolean;
  isAccessible?: boolean;
  hasMonitor?: boolean;
  hasDockingStation?: boolean;
  isNearWindow?: boolean;
  pageNumber?: number;
  pageSize?: number;
}

// Meeting Room types
export interface MeetingRoom {
  id: string;
  floorId: string;
  name: string;
  roomNumber: string;
  capacity: number;
  hasProjector: boolean;
  hasWhiteboard: boolean;
  hasVideoConference: boolean;
  hasTelephone: boolean;
  isAccessible: boolean;
  isAvailable: boolean;
  equipment?: string;
  notes?: string;
  floorName?: string;
  buildingName?: string;
  createdAt: string;
}

export interface CreateMeetingRoomRequest {
  floorId: string;
  name: string;
  roomNumber: string;
  capacity: number;
  hasProjector: boolean;
  hasWhiteboard: boolean;
  hasVideoConference: boolean;
  hasTelephone: boolean;
  isAccessible: boolean;
  equipment?: string;
  notes?: string;
}

export interface UpdateMeetingRoomRequest {
  name?: string;
  roomNumber?: string;
  capacity?: number;
  hasProjector?: boolean;
  hasWhiteboard?: boolean;
  hasVideoConference?: boolean;
  hasTelephone?: boolean;
  isAccessible?: boolean;
  isAvailable?: boolean;
  equipment?: string;
  notes?: string;
}

export interface MeetingRoomSearchRequest {
  buildingId?: string;
  floorId?: string;
  isAvailable?: boolean;
  isAccessible?: boolean;
  minCapacity?: number;
  hasProjector?: boolean;
  hasWhiteboard?: boolean;
  hasVideoConference?: boolean;
  pageNumber?: number;
  pageSize?: number;
}

class SpaceService {
  // ==================== BUILDINGS ====================
  async getBuildings(pageNumber = 1, pageSize = 20): Promise<ApiResponse<PagedResponse<Building>>> {
    const response = await apiClient.get<ApiResponse<PagedResponse<Building>>>(
      `/api/buildings?pageNumber=${pageNumber}&pageSize=${pageSize}`
    );
    return response.data;
  }

  async getBuilding(id: string): Promise<ApiResponse<Building>> {
    const response = await apiClient.get<ApiResponse<Building>>(`/api/buildings/${id}`);
    return response.data;
  }

  async createBuilding(request: CreateBuildingRequest): Promise<ApiResponse<Building>> {
    const response = await apiClient.post<ApiResponse<Building>>('/api/buildings', request);
    return response.data;
  }

  async updateBuilding(id: string, request: UpdateBuildingRequest): Promise<ApiResponse<Building>> {
    const response = await apiClient.put<ApiResponse<Building>>(`/api/buildings/${id}`, request);
    return response.data;
  }

  async deleteBuilding(id: string): Promise<ApiResponse<boolean>> {
    const response = await apiClient.delete<ApiResponse<boolean>>(`/api/buildings/${id}`);
    return response.data;
  }

  // ==================== FLOORS ====================
  async getFloors(buildingId?: string, pageNumber = 1, pageSize = 20): Promise<ApiResponse<PagedResponse<Floor>>> {
    const params = new URLSearchParams({
      pageNumber: pageNumber.toString(),
      pageSize: pageSize.toString(),
    });
    if (buildingId) {
      params.append('buildingId', buildingId);
    }
    const response = await apiClient.get<ApiResponse<PagedResponse<Floor>>>(`/api/floors?${params}`);
    return response.data;
  }

  async getFloor(id: string): Promise<ApiResponse<Floor>> {
    const response = await apiClient.get<ApiResponse<Floor>>(`/api/floors/${id}`);
    return response.data;
  }

  async createFloor(request: CreateFloorRequest): Promise<ApiResponse<Floor>> {
    const response = await apiClient.post<ApiResponse<Floor>>('/api/floors', request);
    return response.data;
  }

  async updateFloor(id: string, request: UpdateFloorRequest): Promise<ApiResponse<Floor>> {
    const response = await apiClient.put<ApiResponse<Floor>>(`/api/floors/${id}`, request);
    return response.data;
  }

  async deleteFloor(id: string): Promise<ApiResponse<boolean>> {
    const response = await apiClient.delete<ApiResponse<boolean>>(`/api/floors/${id}`);
    return response.data;
  }

  // ==================== DESKS ====================
  async searchDesks(request: DeskSearchRequest): Promise<ApiResponse<PagedResponse<Desk>>> {
    const response = await apiClient.post<ApiResponse<PagedResponse<Desk>>>('/api/desks/search', request);
    return response.data;
  }

  async getDesk(id: string): Promise<ApiResponse<Desk>> {
    const response = await apiClient.get<ApiResponse<Desk>>(`/api/desks/${id}`);
    return response.data;
  }

  async createDesk(request: CreateDeskRequest): Promise<ApiResponse<Desk>> {
    const response = await apiClient.post<ApiResponse<Desk>>('/api/desks', request);
    return response.data;
  }

  async updateDesk(id: string, request: UpdateDeskRequest): Promise<ApiResponse<Desk>> {
    const response = await apiClient.put<ApiResponse<Desk>>(`/api/desks/${id}`, request);
    return response.data;
  }

  async deleteDesk(id: string): Promise<ApiResponse<boolean>> {
    const response = await apiClient.delete<ApiResponse<boolean>>(`/api/desks/${id}`);
    return response.data;
  }

  // ==================== MEETING ROOMS ====================
  async searchMeetingRooms(request: MeetingRoomSearchRequest): Promise<ApiResponse<PagedResponse<MeetingRoom>>> {
    const response = await apiClient.post<ApiResponse<PagedResponse<MeetingRoom>>>(
      '/api/meetingrooms/search',
      request
    );
    return response.data;
  }

  async getMeetingRoom(id: string): Promise<ApiResponse<MeetingRoom>> {
    const response = await apiClient.get<ApiResponse<MeetingRoom>>(`/api/meetingrooms/${id}`);
    return response.data;
  }

  async createMeetingRoom(request: CreateMeetingRoomRequest): Promise<ApiResponse<MeetingRoom>> {
    const response = await apiClient.post<ApiResponse<MeetingRoom>>('/api/meetingrooms', request);
    return response.data;
  }

  async updateMeetingRoom(id: string, request: UpdateMeetingRoomRequest): Promise<ApiResponse<MeetingRoom>> {
    const response = await apiClient.put<ApiResponse<MeetingRoom>>(`/api/meetingrooms/${id}`, request);
    return response.data;
  }

  async deleteMeetingRoom(id: string): Promise<ApiResponse<boolean>> {
    const response = await apiClient.delete<ApiResponse<boolean>>(`/api/meetingrooms/${id}`);
    return response.data;
  }
}

export default new SpaceService();
