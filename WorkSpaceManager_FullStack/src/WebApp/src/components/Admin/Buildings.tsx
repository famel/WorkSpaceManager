import React, { useEffect, useState } from 'react';
import spaceService, { Building, CreateBuildingRequest, UpdateBuildingRequest } from '../../services/spaceService';
import { handleApiError } from '../../services/api';
import './Admin.css';

const Buildings: React.FC = () => {
  const [buildings, setBuildings] = useState<Building[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editingBuilding, setEditingBuilding] = useState<Building | null>(null);
  
  // Form state
  const [name, setName] = useState('');
  const [address, setAddress] = useState('');
  const [totalFloors, setTotalFloors] = useState(1);

  useEffect(() => {
    loadBuildings();
  }, []);

  const loadBuildings = async () => {
    try {
      setLoading(true);
      const response = await spaceService.getBuildings(1, 100);
      if (response.success && response.data) {
        setBuildings(response.data.items);
      }
    } catch (err) {
      setError(handleApiError(err));
    } finally {
      setLoading(false);
    }
  };

  const openCreateModal = () => {
    setEditingBuilding(null);
    setName('');
    setAddress('');
    setTotalFloors(1);
    setShowModal(true);
  };

  const openEditModal = (building: Building) => {
    setEditingBuilding(building);
    setName(building.name);
    setAddress(building.address);
    setTotalFloors(building.totalFloors);
    setShowModal(true);
  };

  const closeModal = () => {
    setShowModal(false);
    setEditingBuilding(null);
    setError('');
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    try {
      if (editingBuilding) {
        const request: UpdateBuildingRequest = { name, address, totalFloors };
        const response = await spaceService.updateBuilding(editingBuilding.id, request);
        if (response.success) {
          await loadBuildings();
          closeModal();
        } else {
          setError(response.message || 'Failed to update building');
        }
      } else {
        const request: CreateBuildingRequest = { name, address, totalFloors };
        const response = await spaceService.createBuilding(request);
        if (response.success) {
          await loadBuildings();
          closeModal();
        } else {
          setError(response.message || 'Failed to create building');
        }
      }
    } catch (err) {
      setError(handleApiError(err));
    }
  };

  const handleDelete = async (id: string, name: string) => {
    if (!window.confirm(`Are you sure you want to delete "${name}"?`)) return;

    try {
      const response = await spaceService.deleteBuilding(id);
      if (response.success) {
        await loadBuildings();
      } else {
        alert(response.message || 'Failed to delete building');
      }
    } catch (err) {
      alert(handleApiError(err));
    }
  };

  if (loading) {
    return (
      <div className="admin-page">
        <h1>Buildings</h1>
        <div className="loading">Loading buildings...</div>
      </div>
    );
  }

  return (
    <div className="admin-page">
      <div className="page-header">
        <div>
          <h1>Buildings</h1>
          <p>{buildings.length} total building{buildings.length !== 1 ? 's' : ''}</p>
        </div>
        <button onClick={openCreateModal} className="btn-primary">
          + Add Building
        </button>
      </div>

      {error && !showModal && <div className="error-message">{error}</div>}

      <div className="admin-grid">
        {buildings.map((building) => (
          <div key={building.id} className="admin-card">
            <div className="card-header">
              <h3>{building.name}</h3>
              <span className={`status-badge ${building.isActive ? 'status-active' : 'status-inactive'}`}>
                {building.isActive ? 'Active' : 'Inactive'}
              </span>
            </div>

            <div className="card-body">
              <div className="detail-row">
                <span className="label">📍 Address:</span>
                <span className="value">{building.address}</span>
              </div>
              <div className="detail-row">
                <span className="label">🏢 Total Floors:</span>
                <span className="value">{building.totalFloors}</span>
              </div>
              <div className="detail-row">
                <span className="label">🪑 Total Desks:</span>
                <span className="value">{building.totalDesks}</span>
              </div>
              <div className="detail-row">
                <span className="label">🎯 Meeting Rooms:</span>
                <span className="value">{building.totalMeetingRooms}</span>
              </div>
            </div>

            <div className="card-actions">
              <button onClick={() => openEditModal(building)} className="btn-edit">
                Edit
              </button>
              <button onClick={() => handleDelete(building.id, building.name)} className="btn-delete">
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
              <h2>{editingBuilding ? 'Edit Building' : 'Add Building'}</h2>
              <button onClick={closeModal} className="modal-close">×</button>
            </div>

            <form onSubmit={handleSubmit}>
              {error && <div className="error-message">{error}</div>}

              <div className="form-group">
                <label>Building Name *</label>
                <input
                  type="text"
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                  placeholder="e.g., Main Office Building"
                  required
                  className="form-input"
                />
              </div>

              <div className="form-group">
                <label>Address *</label>
                <input
                  type="text"
                  value={address}
                  onChange={(e) => setAddress(e.target.value)}
                  placeholder="e.g., 123 Main Street, Athens"
                  required
                  className="form-input"
                />
              </div>

              <div className="form-group">
                <label>Total Floors *</label>
                <input
                  type="number"
                  value={totalFloors}
                  onChange={(e) => setTotalFloors(parseInt(e.target.value))}
                  min="1"
                  max="100"
                  required
                  className="form-input"
                />
              </div>

              <div className="modal-actions">
                <button type="button" onClick={closeModal} className="btn-secondary">
                  Cancel
                </button>
                <button type="submit" className="btn-primary">
                  {editingBuilding ? 'Update' : 'Create'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default Buildings;
