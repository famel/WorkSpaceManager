import React from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import authService from '../../services/authService';
import './DashboardLayout.css';

interface DashboardLayoutProps {
  children: React.ReactNode;
}

const DashboardLayout: React.FC<DashboardLayoutProps> = ({ children }) => {
  const navigate = useNavigate();
  const location = useLocation();
  const user = authService.getCurrentUser();

  const handleLogout = () => {
    authService.logout();
    navigate('/login');
  };

  const isActive = (path: string) => location.pathname === path;

  return (
    <div className="dashboard-layout">
      <aside className="sidebar">
        <div className="sidebar-header">
          <h2>WorkSpace Manager</h2>
          <p className="user-info">{user?.name || user?.preferred_username}</p>
        </div>

        <nav className="sidebar-nav">
          <Link to="/dashboard" className={isActive('/dashboard') ? 'nav-item active' : 'nav-item'}>
            <span className="icon">📊</span>
            <span>Dashboard</span>
          </Link>

          <Link to="/bookings" className={isActive('/bookings') ? 'nav-item active' : 'nav-item'}>
            <span className="icon">📅</span>
            <span>My Bookings</span>
          </Link>

          <Link to="/book-space" className={isActive('/book-space') ? 'nav-item active' : 'nav-item'}>
            <span className="icon">➕</span>
            <span>Book a Space</span>
          </Link>

          <Link to="/spaces" className={isActive('/spaces') ? 'nav-item active' : 'nav-item'}>
            <span className="icon">🏢</span>
            <span>Browse Spaces</span>
          </Link>

          {authService.hasRole('admin') || authService.hasRole('facility_manager') ? (
            <>
              <div className="nav-divider"></div>
              <div className="nav-section-title">Administration</div>
              
              <Link to="/admin/buildings" className={isActive('/admin/buildings') ? 'nav-item active' : 'nav-item'}>
                <span className="icon">🏗️</span>
                <span>Buildings</span>
              </Link>

              <Link to="/admin/desks" className={isActive('/admin/desks') ? 'nav-item active' : 'nav-item'}>
                <span className="icon">🪑</span>
                <span>Desks</span>
              </Link>

              <Link to="/admin/meeting-rooms" className={isActive('/admin/meeting-rooms') ? 'nav-item active' : 'nav-item'}>
                <span className="icon">🎯</span>
                <span>Meeting Rooms</span>
              </Link>
            </>
          ) : null}
        </nav>

        <div className="sidebar-footer">
          <button onClick={handleLogout} className="logout-btn">
            <span className="icon">🚪</span>
            <span>Logout</span>
          </button>
        </div>
      </aside>

      <main className="main-content">
        <div className="content-wrapper">
          {children}
        </div>
      </main>
    </div>
  );
};

export default DashboardLayout;
