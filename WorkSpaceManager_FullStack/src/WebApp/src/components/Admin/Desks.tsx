import React, { useEffect, useState } from 'react';
import spaceService, { Desk, Floor, CreateDeskRequest, UpdateDeskRequest } from '../../services/spaceService';
import { handleApiError } from '../../services/api';
import './Admin.css';

const Desks: React.FC = () => {
  const [desks, setDesks] = useState<Desk[]>([]);
  const [floors, setFloors] = useState<Floor[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editingDesk, setEditingDesk] = useState<Desk | null>(null);
  
  // Form state
  const [floorId, setFloorId] = useState('');
  const [deskNumber, setDeskNumber] = useState('');
  const [location, setLocation] = useState('');
  const [hasMonitor, setHasMonitor] = useState(false);
  const [hasDockingStation, setHasDockingStation] = useState(false);
  const [isNearWindow, setIsNearWindow] = useState(false);
  const [isAccessible, setIsAccessible] = useState(false);
  const [notes, setNotes] = useState('');

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      const [desksResponse, floorsResponse] = await Promise.all([
        spaceService.searchDesks({ pageSize: 100 }),
        spaceService.getFloors(undefined, 1, 100)
      ]);
      
      if (desksResponse.success && desksResponse.data) {
        setDesks(desksResponse.data.items);
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
    setEditingDesk(null);
    setFloorId('');
    setDeskNumber('');
    setLocation('');
    setHasMonitor(false);
    setHasDockingStation(false);
    setIsNearWindow(false);
    setIsAccessible(false);
    setNotes('');
    setShowModal(true);
  };

  const openEditModal = (desk: Desk) => {
    setEditingDesk(desk);
    setFloorId(desk.floorId);
    setDeskNumber(desk.deskNumber);
    setLocation(desk.location || '');
    setHasMonitor(desk.hasMonitor);
    setHasDockingStation(desk.hasDockingStation);
    setIsNearWindow(desk.isNearWindow);
    setIsAccessible(desk.isAccessible);
    setNotes(desk.notes || '');
    setShowModal(true);
  };

  const closeModal = () => {
    setShowModal(false);
    setEditingDesk(null);
    setError('');
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    try {
      if (editingDesk) {
        const request: UpdateDeskRequest = {
          deskNumber,
          location: location || undefined,
          hasMonitor,
          hasDockingStation,
          isNearWindow,
          isAccessible,
          notes: notes || undefined
        };
        const response = await spaceService.updateDesk(editingDesk.id, request);
        if (response.success) {
          await loadData();
          closeModal();
        } else {
          setError(response.message || 'Failed to update desk');
        }
      } else {
        const request: CreateDeskRequest = {
          floorId,
          deskNumber,
          location: location || undefined,
          hasMonitor,
          hasDockingStation,
          isNearWindow,
          isAccessible,
          notes: notes || undefined
        };
        const response = await spaceService.createDesk(request);
        if (response.success) {
          await loadData();
          closeModal();
        } else {
          setError(response.message || 'Failed to create desk');
        }
      }
    } catch (err) {
      setError(handleApiError(err));
    }
  };

  const handleDelete = async (id: string, deskNumber: string) => {
    if (!window.confirm(`Are you sure you want to delete desk "${deskNumber}"?`)) return;

    try {
      const response = await spaceService.deleteDesk(id);
      if (response.success) {
        await loadData();
      } else {
        alert(response.message || 'Failed to delete desk');
      }
    } catch (err) {
      alert(handleApiError(err));
    }
  };

  if (loading) {
    return (
      <div className="admin-page">
        <h1>Desks</h1>
        <div className="loading">Loading desks...</div>
      </div>
    );
  }

  return (
    <div className="admin-page">
      <div className="page-header">
        <div>
          <h1>Desks</h1>
          <p>{desks.length} total desk{desks.length !== 1 ? 's' : ''}</p>
        </div>
        <button onClick={openCreateModal} className="btn-primary">
          + Add Desk
        </button>
      </div>

      {error && !showModal && <div className="error-message">{error}</div>}

      <div className="admin-grid">
        {desks.map((desk) => (
          <div key={desk.id} className="admin-card">
            <div className="card-header">
              <h3>Desk {desk.deskNumber}</h3>
              <span className={`status-badge ${desk.isAvailable ? 'status-active' : 'status-inactive'}`}>
                {desk.isAvailable ? 'Available' : 'Unavailable'}
              </span>
            </div>

            <div className="card-body">
              <div className="detail-row">
                <span className="label">🏢 Building:</span>
                <span className="value">{desk.buildingName}</span>
              </div>
              <div className="detail-row">
                <span className="label">📍 Floor:</span>
                <span className="value">{desk.floorName}</span>
              </div>
              {desk.location && (
                <div className="detail-row">
                  <span className="label">📌 Location:</span>
                  <span className="value">{desk.location}</span>
                </div>
              )}
              
              <div className="features-grid">
                {desk.hasMonitor && <span className="feature-badge">🖥️ Monitor</span>}
                {desk.hasDockingStation && <span className="feature-badge">🔌 Docking</span>}
                {desk.isNearWindow && <span className="feature-badge">🪟 Window</span>}
                {desk.isAccessible && <span className="feature-badge">♿ Accessible</span>}
              </div>
            </div>

            <div className="card-actions">
              <button onClick={() => openEditModal(desk)} className="btn-edit">
                Edit
              </button>
              <button onClick={() => handleDelete(desk.id, desk.deskNumber)} className="btn-delete">
                Delete
              </button>
            </div>
          </div>
        ))}
      </div>

      {showModal && (
        <div className="modal-overlay" onClick={closeModal}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2>{editingDesk ? 'Edit Desk' : 'Add Desk'}</h2>
              <button onClick={closeModal} className="modal-close">×</button>
            </div>

            <form onSubmit={handleSubmit}>
              {error && <div className="error-message">{error}</div>}

              <div className="form-group">
                <label>Floor *</label>
                <select
                  value={floorId}
                  onChange={(e) => setFloorId(e.target.value)}
                  required
                  disabled={!!editingDesk}
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
                <label>Desk Number *</label>
                <input
                  type="text"
                  value={deskNumber}
                  onChange={(e) => setDeskNumber(e.target.value)}
                  placeholder="e.g., D-101"
                  required
                  className="form-input"
                />
              </div>

              <div className="form-group">
                <label>Location</label>
                <input
                  type="text"
                  value={location}
                  onChange={(e) => setLocation(e.target.value)}
                  placeholder="e.g., Near elevator, Corner"
                  className="form-input"
                />
              </div>

              <div className="checkbox-group">
                <label className="checkbox-label">
                  <input
                    type="checkbox"
                    checked={hasMonitor}
                    onChange={(e) => setHasMonitor(e.target.checked)}
                  />
                  <span>Has Monitor</span>
                </label>

                <label className="checkbox-label">
                  <input
                    type="checkbox"
                    checked={hasDockingStation}
                    onChange={(e) => setHasDockingStation(e.target.checked)}
                  />
                  <span>Has Docking Station</span>
                </label>

                <label className="checkbox-label">
                  <input
                    type="checkbox"
                    checked={isNearWindow}
                    onChange={(e) => setIsNearWindow(e.target.checked)}
                  />
                  <span>Near Window</span>
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
                  {editingDesk ? 'Update' : 'Create'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default Desks;
