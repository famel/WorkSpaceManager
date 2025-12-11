import apiClient, { ApiResponse, PagedResponse } from './api';

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

export interface UpdateBookingRequest {
  startTime?: string;
  endTime?: string;
  purpose?: string;
  notes?: string;
}

export interface BookingSearchRequest {
  resourceType?: 'Desk' | 'MeetingRoom';
  resourceId?: string;
  userId?: string;
  status?: string;
  startDate?: string;
  endDate?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface AvailabilityRequest {
  resourceType: 'Desk' | 'MeetingRoom';
  resourceId: string;
  startTime: string;
  endTime: string;
}

export interface AvailabilityResponse {
  isAvailable: boolean;
  conflictingBookings?: Booking[];
}

class BookingService {
  private readonly basePath = '/api/bookings';

  // Create a new booking
  async createBooking(request: CreateBookingRequest): Promise<ApiResponse<Booking>> {
    const response = await apiClient.post<ApiResponse<Booking>>(this.basePath, request);
    return response.data;
  }

  // Get booking by ID
  async getBooking(id: string): Promise<ApiResponse<Booking>> {
    const response = await apiClient.get<ApiResponse<Booking>>(`${this.basePath}/${id}`);
    return response.data;
  }

  // Get current user's bookings
  async getMyBookings(): Promise<ApiResponse<Booking[]>> {
    const response = await apiClient.get<ApiResponse<Booking[]>>(`${this.basePath}/my-bookings`);
    return response.data;
  }

  // Get upcoming bookings
  async getUpcomingBookings(): Promise<ApiResponse<Booking[]>> {
    const response = await apiClient.get<ApiResponse<Booking[]>>(`${this.basePath}/upcoming`);
    return response.data;
  }

  // Search bookings
  async searchBookings(request: BookingSearchRequest): Promise<ApiResponse<PagedResponse<Booking>>> {
    const response = await apiClient.post<ApiResponse<PagedResponse<Booking>>>(
      `${this.basePath}/search`,
      request
    );
    return response.data;
  }

  // Update booking
  async updateBooking(id: string, request: UpdateBookingRequest): Promise<ApiResponse<Booking>> {
    const response = await apiClient.put<ApiResponse<Booking>>(`${this.basePath}/${id}`, request);
    return response.data;
  }

  // Cancel booking
  async cancelBooking(id: string): Promise<ApiResponse<boolean>> {
    const response = await apiClient.post<ApiResponse<boolean>>(`${this.basePath}/${id}/cancel`);
    return response.data;
  }

  // Check in
  async checkIn(id: string): Promise<ApiResponse<Booking>> {
    const response = await apiClient.post<ApiResponse<Booking>>(`${this.basePath}/${id}/checkin`);
    return response.data;
  }

  // Check out
  async checkOut(id: string): Promise<ApiResponse<Booking>> {
    const response = await apiClient.post<ApiResponse<Booking>>(`${this.basePath}/${id}/checkout`);
    return response.data;
  }

  // Check availability
  async checkAvailability(request: AvailabilityRequest): Promise<ApiResponse<AvailabilityResponse>> {
    const response = await apiClient.post<ApiResponse<AvailabilityResponse>>(
      `${this.basePath}/check-availability`,
      request
    );
    return response.data;
  }
}

export default new BookingService();
