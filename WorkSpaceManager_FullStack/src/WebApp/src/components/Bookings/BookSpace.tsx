import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import bookingService, { CreateBookingRequest } from '../../services/bookingService';
import spaceService, { Desk, MeetingRoom } from '../../services/spaceService';
import { handleApiError } from '../../services/api';
import './BookSpace.css';

const BookSpace: React.FC = () => {
  const navigate = useNavigate();
  const [resourceType, setResourceType] = useState<'Desk' | 'MeetingRoom'>('Desk');
  const [desks, setDesks] = useState<Desk[]>([]);
  const [meetingRooms, setMeetingRooms] = useState<MeetingRoom[]>([]);
  const [selectedResource, setSelectedResource] = useState('');
  const [startTime, setStartTime] = useState('');
  const [endTime, setEndTime] = useState('');
  const [purpose, setPurpose] = useState('');
  const [notes, setNotes] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    loadResources();
  }, [resourceType]);

  const loadResources = async () => {
    try {
      if (resourceType === 'Desk') {
        const response = await spaceService.searchDesks({ isAvailable: true, pageSize: 100 });
        if (response.success && response.data) {
          setDesks(response.data.items);
        }
      } else {
        const response = await spaceService.searchMeetingRooms({ isAvailable: true, pageSize: 100 });
        if (response.success && response.data) {
          setMeetingRooms(response.data.items);
        }
      }
    } catch (err) {
      console.error('Failed to load resources:', err);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const request: CreateBookingRequest = {
        resourceType,
        resourceId: selectedResource,
        startTime,
        endTime,
        purpose: purpose || undefined,
        notes: notes || undefined,
      };

      const response = await bookingService.createBooking(request);
      if (response.success) {
        alert('Booking created successfully!');
        navigate('/bookings');
      } else {
        setError(response.message || 'Failed to create booking');
      }
    } catch (err) {
      setError(handleApiError(err));
    } finally {
      setLoading(false);
    }
  };

  const resources = resourceType === 'Desk' ? desks : meetingRooms;

  return (
    <div className="book-space">
      <div className="page-header">
        <h1>Book a Space</h1>
        <p>Reserve a desk or meeting room for your work</p>
      </div>

      <form onSubmit={handleSubmit} className="booking-form">
        {error && <div className="error-message">{error}</div>}

        <div className="form-section">
          <h3>Resource Type</h3>
          <div className="resource-type-selector">
            <button
              type="button"
              className={resourceType === 'Desk' ? 'type-btn active' : 'type-btn'}
              onClick={() => {
                setResourceType('Desk');
                setSelectedResource('');
              }}
            >
              <span className="icon">🪑</span>
              <span>Desk</span>
            </button>
            <button
              type="button"
              className={resourceType === 'MeetingRoom' ? 'type-btn active' : 'type-btn'}
              onClick={() => {
                setResourceType('MeetingRoom');
                setSelectedResource('');
              }}
            >
              <span className="icon">🎯</span>
              <span>Meeting Room</span>
            </button>
          </div>
        </div>

        <div className="form-section">
          <h3>Select {resourceType === 'Desk' ? 'Desk' : 'Meeting Room'}</h3>
          <select
            value={selectedResource}
            onChange={(e) => setSelectedResource(e.target.value)}
            required
            className="form-select"
          >
            <option value="">-- Select {resourceType} --</option>
            {resources.map((resource: any) => (
              <option key={resource.id} value={resource.id}>
                {resourceType === 'Desk'
                  ? `Desk ${resource.deskNumber} - ${resource.buildingName} - ${resource.floorName}`
                  : `${resource.name} (Capacity: ${resource.capacity}) - ${resource.buildingName}`}
              </option>
            ))}
          </select>
        </div>

        <div className="form-row">
          <div className="form-group">
            <label>Start Time</label>
            <input
              type="datetime-local"
              value={startTime}
              onChange={(e) => setStartTime(e.target.value)}
              required
              className="form-input"
            />
          </div>

          <div className="form-group">
            <label>End Time</label>
            <input
              type="datetime-local"
              value={endTime}
              onChange={(e) => setEndTime(e.target.value)}
              required
              className="form-input"
            />
          </div>
        </div>

        <div className="form-group">
          <label>Purpose (Optional)</label>
          <input
            type="text"
            value={purpose}
            onChange={(e) => setPurpose(e.target.value)}
            placeholder="e.g., Team meeting, Focus work"
            className="form-input"
          />
        </div>

        <div className="form-group">
          <label>Notes (Optional)</label>
          <textarea
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
            placeholder="Any additional notes..."
            rows={3}
            className="form-textarea"
          />
        </div>

        <div className="form-actions">
          <button type="button" onClick={() => navigate('/bookings')} className="btn-secondary">
            Cancel
          </button>
          <button type="submit" disabled={loading} className="btn-primary">
            {loading ? 'Creating Booking...' : 'Create Booking'}
          </button>
        </div>
      </form>
    </div>
  );
};

export default BookSpace;
