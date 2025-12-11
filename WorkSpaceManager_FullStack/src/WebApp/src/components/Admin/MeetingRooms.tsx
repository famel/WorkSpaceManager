import React, { useEffect, useState } from 'react';
import spaceService, { MeetingRoom, Floor, CreateMeetingRoomRequest, UpdateMeetingRoomRequest } from '../../services/spaceService';
import { handleApiError } from '../../services/api';
import './Admin.css';

const MeetingRooms: React.FC = () => {
  const [meetingRooms, setMeetingRooms] = useState<MeetingRoom[]>([]);
  const [floors, setFloors] = useState<Floor[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editingRoom, setEditingRoom] = useState<MeetingRoom | null>(null);
  
  // Form state
  const [floorId, setFloorId] = useState('');
  const [name, setName] = useState('');
  const [roomNumber, setRoomNumber] = useState('');
  const [capacity, setCapacity] = useState(4);
  const [hasProjector, setHasProjector] = useState(false);
  const [hasWhiteboard, setHasWhiteboard] = useState(false);
  const [hasVideoConference, setHasVideoConference] = useState(false);
  const [hasTelephone, setHasTelephone] = useState(false);
  const [isAccessible, setIsAccessible] = useState(false);
  const [equipment, setEquipment] = useState('');
  const [notes, setNotes] = useState('');

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      const [roomsResponse, floorsResponse] = await Promise.all([
        spaceService.searchMeetingRooms({ pageSize: 100 }),
        spaceService.getFloors(undefined, 1, 100)
      ]);
      
      if (roomsResponse.success && roomsResponse.data) {
        setMeetingRooms(roomsResponse.data.items);
      }
      if (floorsResponse.success && floorsResponse.data) {
        setFloors(floorsResponse.data.items);
      }
    } catch (err) {
      setError(handleApiError(err));
    } finally {
      setLoading(false);
    }
  };

  const openCreateModal = () => {
    setEditingRoom(null);
    setFloorId('');
    setName('');
    setRoomNumber('');
    setCapacity(4);
    setHasProjector(false);
    setHasWhiteboard(false);
    setHasVideoConference(false);
    setHasTelephone(false);
    setIsAccessible(false);
    setEquipment('');
    setNotes('');
    setShowModal(true);
  };

  const openEditModal = (room: MeetingRoom) => {
    setEditingRoom(room);
    setFloorId(room.floorId);
    setName(room.name);
    setRoomNumber(room.roomNumber);
    setCapacity(room.capacity);
    setHasProjector(room.hasProjector);
    setHasWhiteboard(room.hasWhiteboard);
    setHasVideoConference(room.hasVideoConference);
    setHasTelephone(room.hasTelephone);
    setIsAccessible(room.isAccessible);
    setEquipment(room.equipment || '');
    setNotes(room.notes || '');
    setShowModal(true);
  };

  const closeModal = () => {
    setShowModal(false);
    setEditingRoom(null);
    setError('');
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    try {
      if (editingRoom) {
        const request: UpdateMeetingRoomRequest = {
          name,
          roomNumber,
          capacity,
          hasProjector,
          hasWhiteboard,
          hasVideoConference,
          hasTelephone,
          isAccessible,
          equipment: equipment || undefined,
          notes: notes || undefined
        };
        const response = await spaceService.updateMeetingRoom(editingRoom.id, request);
        if (response.success) {
          await loadData();
          closeModal();
        } else {
          setError(response.message || 'Failed to update meeting room');
        }
      } else {
        const request: CreateMeetingRoomRequest = {
          floorId,
          name,
          roomNumber,
          capacity,
          hasProjector,
          hasWhiteboard,
          hasVideoConference,
          hasTelephone,
          isAccessible,
          equipment: equipment || undefined,
          notes: notes || undefined
        };
        const response = await spaceService.createMeetingRoom(request);
        if (response.success) {
          await loadData();
          closeModal();
        } else {
          setError(response.message || 'Failed to create meeting room');
        }
      }
    } catch (err) {
      setError(handleApiError(err));
    }
  };

  const handleDelete = async (id: string, name: string) => {
    if (!window.confirm(`Are you sure you want to delete "${name}"?`)) return;

    try {
      const response = await spaceService.deleteMeetingRoom(id);
      if (response.success) {
        await loadData();
      } else {
        alert(response.message || 'Failed to delete meeting room');
      }
    } catch (err) {
      alert(handleApiError(err));
    }
  };

  if (loading) {
    return (
      <div className="admin-page">
        <h1>Meeting Rooms</h1>
        <div className="loading">Loading meeting rooms...</div>
      </div>
    );
  }

  return (
    <div className="admin-page">
      <div className="page-header">
        <div>
          <h1>Meeting Rooms</h1>
          <p>{meetingRooms.length} total room{meetingRooms.length !== 1 ? 's' : ''}</p>
        </div>
        <button onClick={openCreateModal} className="btn-primary">
          + Add Meeting Room
        </button>
      </div>

      {error && !showModal && <div className="error-message">{error}</div>}

      <div className="admin-grid">
        {meetingRooms.map((room) => (
          <div key={room.id} className="admin-card">
            <div className="card-header">
              <h3>{room.name}</h3>
              <span className={`status-badge ${room.isAvailable ? 'status-active' : 'status-inactive'}`}>
                {room.isAvailable ? 'Available' : 'Unavailable'}
              </span>
            </div>

            <div className="card-body">
              <div className="detail-row">
                <span className="label">🏢 Building:</span>
                <span className="value">{room.buildingName}</span>
              </div>
              <div className="detail-row">
                <span className="label">📍 Floor:</span>
                <span className="value">{room.floorName}</span>
              </div>
              <div className="detail-row">
                <span className="label">🔢 Room Number:</span>
                <span className="value">{room.roomNumber}</span>
              </div>
              <div className="detail-row">
                <span className="label">👥 Capacity:</span>
                <span className="value">{room.capacity} people</span>
              </div>
              
              <div className="features-grid">
                {room.hasProjector && <span className="feature-badge">📽️ Projector</span>}
                {room.hasWhiteboard && <span className="feature-badge">📝 Whiteboard</span>}
                {room.hasVideoConference && <span className="feature-badge">📹 Video Conf</span>}
                {room.hasTelephone && <span className="feature-badge">📞 Phone</span>}
                {room.isAccessible && <span className="feature-badge">♿ Accessible</span>}
              </div>
            </div>

            <div className="card-actions">
              <button onClick={() => openEditModal(room)} className="btn-edit">
                Edit
              </button>
              <button onClick={() => handleDelete(room.id, room.name)} className="btn-delete">
                Delete
              </button>
            </div>
          </div>
        ))}
      </div>

      {showModal && (
        <div className="modal-overlay" onClick={closeModal}>
          <div className="modal-content modal-large" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2>{editingRoom ? 'Edit Meeting Room' : 'Add Meeting Room'}</h2>
              <button onClick={closeModal} className="modal-close">×</button>
            </div>

            <form onSubmit={handleSubmit}>
              {error && <div className="error-message">{error}</div>}

              <div className="form-row">
                <div className="form-group">
                  <label>Floor *</label>
                  <select
                    value={floorId}
                    onChange={(e) => setFloorId(e.target.value)}
                    required
                    disabled={!!editingRoom}
                    className="form-select"
                  >
                    <option value="">-- Select Floor --</option>
                    {floors.map((floor) => (
                      <option key={floor.id} value={floor.id}>
                        {floor.buildingName} - {floor.name}
                      </option>
                    ))}
                  </select>
                </div>

                <div className="form-group">
                  <label>Room Number *</label>
                  <input
                    type="text"
                    value={roomNumber}
                    onChange={(e) => setRoomNumber(e.target.value)}
                    placeholder="e.g., MR-301"
                    required
                    className="form-input"
                  />
                </div>
              </div>

              <div className="form-row">
                <div className="form-group">
                  <label>Room Name *</label>
                  <input
                    type="text"
                    value={name}
                    onChange={(e) => setName(e.target.value)}
                    placeholder="e.g., Executive Board Room"
                    required
                    className="form-input"
                  />
                </div>

                <div className="form-group">
                  <label>Capacity *</label>
                  <input
                    type="number"
                    value={capacity}
                    onChange={(e) => setCapacity(parseInt(e.target.value))}
                    min="1"
                    max="100"
                    required
                    className="form-input"
                  />
                </div>
              </div>

              <div className="checkbox-group">
                <label className="checkbox-label">
                  <input
                    type="checkbox"
                    checked={hasProjector}
                    onChange={(e) => setHasProjector(e.target.checked)}
                  />
                  <span>Has Projector</span>
                </label>

                <label className="checkbox-label">
                  <input
                    type="checkbox"
                    checked={hasWhiteboard}
                    onChange={(e) => setHasWhiteboard(e.target.checked)}
                  />
                  <span>Has Whiteboard</span>
                </label>

                <label className="checkbox-label">
                  <input
                    type="checkbox"
                    checked={hasVideoConference}
                    onChange={(e) => setHasVideoConference(e.target.checked)}
                  />
                  <span>Has Video Conference</span>
                </label>

                <label className="checkbox-label">
                  <input
                    type="checkbox"
                    checked={hasTelephone}
                    onChange={(e) => setHasTelephone(e.target.checked)}
                  />
                  <span>Has Telephone</span>
                </label>

                <label className="checkbox-label">
                  <input
                    type="checkbox"
                    checked={isAccessible}
                    onChange={(e) => setIsAccessible(e.target.checked)}
                  />
                  <span>Wheelchair Accessible</span>
                </label>
              </div>

              <div className="form-group">
                <label>Equipment</label>
                <input
                  type="text"
                  value={equipment}
                  onChange={(e) => setEquipment(e.target.value)}
                  placeholder="e.g., 4K TV, Sound system, Wireless presentation"
                  className="form-input"
                />
              </div>

              <div className="form-group">
                <label>Notes</label>
                <textarea
                  value={notes}
                  onChange={(e) => setNotes(e.target.value)}
                  placeholder="Any additional notes..."
                  rows={3}
                  className="form-textarea"
                />
              </div>

              <div className="modal-actions">
                <button type="button" onClick={closeModal} className="btn-secondary">
                  Cancel
                </button>
                <button type="submit" className="btn-primary">
                  {editingRoom ? 'Update' : 'Create'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default MeetingRooms;
