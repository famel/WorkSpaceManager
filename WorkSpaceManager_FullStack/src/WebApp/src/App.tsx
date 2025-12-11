import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import Login from './components/Auth/Login';
import ProtectedRoute from './components/Auth/ProtectedRoute';
import DashboardLayout from './components/Layout/DashboardLayout';
import Dashboard from './components/Dashboard/Dashboard';
import MyBookings from './components/Bookings/MyBookings';
import BookSpace from './components/Bookings/BookSpace';
import Buildings from './components/Admin/Buildings';
import Desks from './components/Admin/Desks';
import MeetingRooms from './components/Admin/MeetingRooms';
import './App.css';

const App: React.FC = () => {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<Login />} />
        
        <Route
          path="/dashboard"
          element={
            <ProtectedRoute>
              <DashboardLayout>
                <Dashboard />
              </DashboardLayout>
            </ProtectedRoute>
          }
        />

        <Route
          path="/bookings"
          element={
            <ProtectedRoute>
              <DashboardLayout>
                <MyBookings />
              </DashboardLayout>
            </ProtectedRoute>
          }
        />

        <Route
          path="/book-space"
          element={
            <ProtectedRoute>
              <DashboardLayout>
                <BookSpace />
              </DashboardLayout>
            </ProtectedRoute>
          }
        />

        <Route
          path="/admin/buildings"
          element={
            <ProtectedRoute requiredRole="admin">
              <DashboardLayout>
                <Buildings />
              </DashboardLayout>
            </ProtectedRoute>
          }
        />

        <Route
          path="/admin/desks"
          element={
            <ProtectedRoute requiredRole="admin">
              <DashboardLayout>
                <Desks />
              </DashboardLayout>
            </ProtectedRoute>
          }
        />

        <Route
          path="/admin/meeting-rooms"
          element={
            <ProtectedRoute requiredRole="admin">
              <DashboardLayout>
                <MeetingRooms />
              </DashboardLayout>
            </ProtectedRoute>
          }
        />

        <Route path="/" element={<Navigate to="/dashboard" replace />} />
        <Route path="*" element={<Navigate to="/dashboard" replace />} />
      </Routes>
    </BrowserRouter>
  );
};

export default App;
