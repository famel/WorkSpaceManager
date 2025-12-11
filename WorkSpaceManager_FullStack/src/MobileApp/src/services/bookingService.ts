import apiClient, { ApiResponse, PagedResponse } from './apiClient';

// Booking types
export interface Booking {
  id: string;
  userId: string;
  userName?: string;
  resourceType: 'Desk' | 'MeetingRoom';
  resourceId: string;
  resourceName?: string;
  startTime: string;
  endTime: string;
  status: 'Pending' | 'Confirmed' | 'CheckedIn' | 'CheckedOut' | 'Cancelled' | 'NoShow';
  purpose?: string;
  notes?: string;
  checkInTime?: string;
  checkOutTime?: string;
  createdAt: string;
}

export interface CreateBookingRequest {
  resourceType: 'Desk' | 'MeetingRoom';
  resourceId: string;
  startTime: string;
  endTime: string;
  purpose?: string;
  notes?: string;
}

class BookingService {
  private readonly basePath = '/api/bookings';

  async createBooking(request: CreateBookingRequest): Promise<ApiResponse<Booking>> {
    const response = await apiClient.post<ApiResponse<Booking>>(this.basePath, request);
    return response.data;
  }

  async getMyBookings(): Promise<ApiResponse<Booking[]>> {
    const response = await apiClient.get<ApiResponse<Booking[]>>(`${this.basePath}/my-bookings`);
    return response.data;
  }

  async getUpcomingBookings(): Promise<ApiResponse<Booking[]>> {
    const response = await apiClient.get<ApiResponse<Booking[]>>(`${this.basePath}/upcoming`);
    return response.data;
  }

  async checkIn(id: string): Promise<ApiResponse<Booking>> {
    const response = await apiClient.post<ApiResponse<Booking>>(`${this.basePath}/${id}/checkin`);
    return response.data;
  }

  async checkOut(id: string): Promise<ApiResponse<Booking>> {
    const response = await apiClient.post<ApiResponse<Booking>>(`${this.basePath}/${id}/checkout`);
    return response.data;
  }

  async cancelBooking(id: string): Promise<ApiResponse<boolean>> {
    const response = await apiClient.post<ApiResponse<boolean>>(`${this.basePath}/${id}/cancel`);
    return response.data;
  }
}

export default new BookingService();
