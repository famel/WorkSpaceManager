import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import bookingService, { Booking } from '../../services/bookingService';
import authService from '../../services/authService';
import './Dashboard.css';

const Dashboard: React.FC = () => {
  const [upcomingBookings, setUpcomingBookings] = useState<Booking[]>([]);
  const [loading, setLoading] = useState(true);
  const user = authService.getCurrentUser();

  useEffect(() => {
    loadUpcomingBookings();
  }, []);

  const loadUpcomingBookings = async () => {
    try {
      const response = await bookingService.getUpcomingBookings();
      if (response.success && response.data) {
        setUpcomingBookings(response.data.slice(0, 5)); // Show only first 5
      }
    } catch (err) {
      console.error('Failed to load upcoming bookings:', err);
    } finally {
      setLoading(false);
    }
  };

  const formatDateTime = (dateString: string) => {
    return new Date(dateString).toLocaleString('en-US', {
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  return (
    <div className="dashboard">
      <div className="dashboard-header">
        <div>
          <h1>Welcome back, {user?.name || user?.preferred_username}!</h1>
          <p>Here's what's happening with your workspace bookings</p>
        </div>
      </div>

      <div className="stats-grid">
        <div className="stat-card">
          <div className="stat-icon">📅</div>
          <div className="stat-content">
            <h3>{upcomingBookings.length}</h3>
            <p>Upcoming Bookings</p>
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon">✅</div>
          <div className="stat-content">
            <h3>{upcomingBookings.filter(b => b.status === 'Confirmed').length}</h3>
            <p>Confirmed</p>
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon">🏢</div>
          <div className="stat-content">
            <h3>{new Set(upcomingBookings.map(b => b.resourceType)).size}</h3>
            <p>Resource Types</p>
          </div>
        </div>
      </div>

      <div className="quick-actions">
        <h2>Quick Actions</h2>
        <div className="actions-grid">
          <Link to="/book-space" className="action-card">
            <span className="action-icon">➕</span>
            <h3>Book a Space</h3>
            <p>Reserve a desk or meeting room</p>
          </Link>

          <Link to="/bookings" className="action-card">
            <span className="action-icon">📋</span>
            <h3>My Bookings</h3>
            <p>View and manage your bookings</p>
          </Link>

          <Link to="/spaces" className="action-card">
            <span className="action-icon">🔍</span>
            <h3>Browse Spaces</h3>
            <p>Explore available workspaces</p>
          </Link>
        </div>
      </div>

      <div className="upcoming-section">
        <div className="section-header">
          <h2>Upcoming Bookings</h2>
          <Link to="/bookings" className="view-all-link">View All →</Link>
        </div>

        {loading ? (
          <div className="loading">Loading...</div>
        ) : upcomingBookings.length === 0 ? (
          <div className="empty-state">
            <p>No upcoming bookings</p>
            <Link to="/book-space" className="btn-primary">Book Your First Space</Link>
          </div>
        ) : (
          <div className="bookings-list">
            {upcomingBookings.map((booking) => (
              <div key={booking.id} className="booking-item">
                <div className="booking-info">
                  <h4>{booking.resourceName || `${booking.resourceType} ${booking.resourceId.substring(0, 8)}`}</h4>
                  <p className="booking-time">
                    {formatDateTime(booking.startTime)} - {formatDateTime(booking.endTime)}
                  </p>
                  {booking.purpose && <p className="booking-purpose">{booking.purpose}</p>}
                </div>
                <span className={`status-badge status-${booking.status.toLowerCase()}`}>
                  {booking.status}
                </span>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default Dashboard;
