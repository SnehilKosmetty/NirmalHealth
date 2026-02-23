import { useEffect, useState } from 'react';
import { useAuth } from '../../context/AuthContext';
import { api, type DoctorListItem } from '../../api/client';

export default function AdminDoctors() {
  const { user } = useAuth();
  const [list, setList] = useState<DoctorListItem[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api.admin.doctors.list(user?.hospitalId ?? undefined)
      .then(setList)
      .catch(() => setList([]))
      .finally(() => setLoading(false));
  }, [user?.hospitalId]);

  if (loading) return <div className="container page">Loading...</div>;

  return (
    <div className="container page">
      <h1 className="page-title">Admin · Doctors</h1>
      <p className="page-subtitle">
        {user?.hospitalId ? 'Doctors at your hospital' : 'All doctors'}
      </p>
      <div className="admin-list">
        {list.map((d) => (
          <div key={d.id} className="card" style={{ padding: '1rem', marginBottom: '0.75rem' }}>
            <strong>{d.fullName}</strong>
            <p style={{ margin: '0.25rem 0 0', fontSize: '0.9rem', color: 'var(--color-text-muted)' }}>
              {d.hospitalName} · {d.specialties.join(', ')} · {d.supportsInPerson && 'In-person'} {d.supportsVideo && 'Video'}
            </p>
          </div>
        ))}
      </div>
      {list.length === 0 && <p className="empty-msg">No doctors.</p>}
    </div>
  );
}
