import { Link } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';

export default function AdminDashboard() {
  const { user } = useAuth();
  const isSuperAdmin = user?.roles.includes('SuperAdmin');
  const hospitalId = user?.hospitalId;

  return (
    <div className="container page">
      <h1 className="page-title">Admin Dashboard</h1>
      <p className="page-subtitle">Welcome, {user?.fullName}. Role: {user?.roles.join(', ')}</p>

      <div className="admin-cards" style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(240px, 1fr))', gap: '1rem', marginTop: '1.5rem' }}>
        <Link to="/admin/hospitals" className="card" style={{ padding: '1.5rem', textDecoration: 'none', color: 'inherit' }}>
          <h3 style={{ margin: '0 0 0.5rem' }}>🏥 Hospitals</h3>
          <p style={{ margin: 0, fontSize: '0.9rem', color: 'var(--color-text-muted)' }}>View and edit hospitals</p>
        </Link>
        <Link to="/admin/doctors" className="card" style={{ padding: '1.5rem', textDecoration: 'none', color: 'inherit' }}>
          <h3 style={{ margin: '0 0 0.5rem' }}>👨‍⚕️ Doctors</h3>
          <p style={{ margin: 0, fontSize: '0.9rem', color: 'var(--color-text-muted)' }}>Manage doctors</p>
        </Link>
        <Link to="/admin/appointments" className="card" style={{ padding: '1.5rem', textDecoration: 'none', color: 'inherit' }}>
          <h3 style={{ margin: '0 0 0.5rem' }}>📅 Appointments</h3>
          <p style={{ margin: 0, fontSize: '0.9rem', color: 'var(--color-text-muted)' }}>
            View bookings{hospitalId ? ' for your hospital' : ''}
          </p>
        </Link>
        {isSuperAdmin && (
          <div className="card" style={{ padding: '1.5rem' }}>
            <h3 style={{ margin: '0 0 0.5rem' }}>👤 Create hospital admin</h3>
            <p style={{ margin: 0, fontSize: '0.9rem', color: 'var(--color-text-muted)' }}>Add staff in Hospitals page</p>
          </div>
        )}
      </div>
    </div>
  );
}
