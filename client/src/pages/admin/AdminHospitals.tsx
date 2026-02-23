import { useEffect, useState } from 'react';
import { useAuth } from '../../context/AuthContext';
import { api, type HospitalListItem } from '../../api/client';

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

  if (loading) return <div className="container page">Loading...</div>;

  return (
    <div className="container page">
      <h1 className="page-title">Admin · Hospitals</h1>
      <p className="page-subtitle">
        {user?.roles.includes('SuperAdmin') ? 'All hospitals' : 'Your hospital'}
      </p>
      <div className="admin-list">
        {list.map((h) => (
          <div key={h.id} className="card" style={{ padding: '1rem', marginBottom: '0.75rem' }}>
            <strong>{h.name}</strong>
            <p style={{ margin: '0.25rem 0 0', fontSize: '0.9rem', color: 'var(--color-text-muted)' }}>
              {h.address} · {h.phone} · {h.bedCount} beds
            </p>
          </div>
        ))}
      </div>
      {list.length === 0 && <p className="empty-msg">No hospitals.</p>}
    </div>
  );
}
