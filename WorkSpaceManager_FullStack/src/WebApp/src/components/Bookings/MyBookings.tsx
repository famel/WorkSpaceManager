import React, { useEffect, useState } from 'react';
import bookingService, { Booking } from '../../services/bookingService';
import { handleApiError } from '../../services/api';
import './MyBookings.css';

const MyBookings: React.FC = () => {
  const [bookings, setBookings] = useState<Booking[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    loadBookings();
  }, []);

  const loadBookings = async () => {
    try {
      setLoading(true);
      const response = await bookingService.getMyBookings();
      if (response.success && response.data) {
        setBookings(response.data);
      }
    } catch (err) {
      setError(handleApiError(err));
    } finally {
      setLoading(false);
    }
  };

  const handleCheckIn = async (id: string) => {
    try {
      const response = await bookingService.checkIn(id);
      if (response.success) {
        loadBookings();
      }
    } catch (err) {
      alert(handleApiError(err));
    }
  };

  const handleCheckOut = async (id: string) => {
    try {
      const response = await bookingService.checkOut(id);
      if (response.success) {
        loadBookings();
      }
    } catch (err) {
      alert(handleApiError(err));
    }
  };

  const handleCancel = async (id: string) => {
    if (!window.confirm('Are you sure you want to cancel this booking?')) return;

    try {
      const response = await bookingService.cancelBooking(id);
      if (response.success) {
        loadBookings();
      }
    } catch (err) {
      alert(handleApiError(err));
    }
  };

  const getStatusBadge = (status: string) => {
    const statusClasses: Record<string, string> = {
      Pending: 'status-pending',
      Confirmed: 'status-confirmed',
      CheckedIn: 'status-checkedin',
      CheckedOut: 'status-checkedout',
      Cancelled: 'status-cancelled',
      NoShow: 'status-noshow',
    };
    return <span className={`status-badge ${statusClasses[status] || ''}`}>{status}</span>;
  };

  const formatDateTime = (dateString: string) => {
    return new Date(dateString).toLocaleString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  if (loading) {
    return (
      <div className="my-bookings">
        <h1>My Bookings</h1>
        <div className="loading">Loading your bookings...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="my-bookings">
        <h1>My Bookings</h1>
        <div className="error-message">{error}</div>
      </div>
    );
  }

  return (
    <div className="my-bookings">
      <div className="page-header">
        <h1>My Bookings</h1>
        <p>{bookings.length} total booking{bookings.length !== 1 ? 's' : ''}</p>
      </div>

      {bookings.length === 0 ? (
        <div className="empty-state">
          <p>You don't have any bookings yet.</p>
          <a href="/book-space" className="btn-primary">Book a Space</a>
        </div>
      ) : (
        <div className="bookings-grid">
          {bookings.map((booking) => (
            <div key={booking.id} className="booking-card">
              <div className="booking-header">
                <div>
                  <h3>{booking.resourceName || `${booking.resourceType} ${booking.resourceId.substring(0, 8)}`}</h3>
                  <p className="resource-type">{booking.resourceType}</p>
                </div>
                {getStatusBadge(booking.status)}
              </div>

              <div className="booking-details">
                <div className="detail-row">
                  <span className="label">Start:</span>
                  <span className="value">{formatDateTime(booking.startTime)}</span>
                </div>
                <div className="detail-row">
                  <span className="label">End:</span>
                  <span className="value">{formatDateTime(booking.endTime)}</span>
                </div>
                {booking.purpose && (
                  <div className="detail-row">
                    <span className="label">Purpose:</span>
                    <span className="value">{booking.purpose}</span>
                  </div>
                )}
                {booking.checkInTime && (
                  <div className="detail-row">
                    <span className="label">Checked In:</span>
                    <span className="value">{formatDateTime(booking.checkInTime)}</span>
                  </div>
                )}
              </div>

              <div className="booking-actions">
                {booking.status === 'Confirmed' && (
                  <>
                    <button onClick={() => handleCheckIn(booking.id)} className="btn-success">
                      Check In
                    </button>
                    <button onClick={() => handleCancel(booking.id)} className="btn-danger">
                      Cancel
                    </button>
                  </>
                )}
                {booking.status === 'CheckedIn' && (
                  <button onClick={() => handleCheckOut(booking.id)} className="btn-primary">
                    Check Out
                  </button>
                )}
                {booking.status === 'Pending' && (
                  <button onClick={() => handleCancel(booking.id)} className="btn-danger">
                    Cancel
                  </button>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default MyBookings;
