import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { api, type HospitalListItem } from '../../api/client';
import './Admin.css';

export default function AdminHospitals() {
  const { user } = useAuth();
  const [list, setList] = useState<HospitalListItem[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api.admin.hospitals.list()
      .then(setList)
      .catch(() => setList([]))
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <div className="container page admin-page admin-loading">Loading hospitals…</div>;

  return (
    <div className="container page admin-page">
      <Link to="/admin" className="admin-back-link">← Back to Dashboard</Link>
      <h1 className="page-title">Hospitals</h1>
      <p className="page-subtitle">
        {user?.roles.includes('SuperAdmin') ? 'All verified hospitals. Click for full details.' : 'Your hospital — verified details.'}
      </p>
      {list.length === 0 ? (
        <p className="admin-empty">No hospitals.</p>
      ) : (
        <div className="admin-list-grid">
          {list.map((h) => (
            <Link key={h.id} to={`/admin/hospitals/${h.id}`} className="admin-list-card">
              <div className="card-name">{h.name}</div>
              <p className="card-meta">
                {h.address}
                {h.area && ` · ${h.area}`}
                <br />
                📞 {h.phone} · 🛏 {h.bedCount} beds
              </p>
              <div className="card-badges">
                <span className={`admin-badge ${h.type === 'Government' ? 'type-gov' : ''}`}>{h.type}</span>
                {h.isEmergency && <span className="admin-badge emergency">Emergency</span>}
                {h.specialties?.length ? <span className="admin-badge">{h.specialties.length} specialties</span> : null}
              </div>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
