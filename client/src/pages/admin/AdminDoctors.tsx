import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { api, type DoctorListItem } from '../../api/client';
import './Admin.css';

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

  if (loading) return <div className="container page admin-page admin-loading">Loading doctors…</div>;

  return (
    <div className="container page admin-page">
      <Link to="/admin" className="admin-back-link">← Back to Dashboard</Link>
      <h1 className="page-title">Doctors</h1>
      <p className="page-subtitle">
        {user?.hospitalId ? 'Verified doctors at your hospital. Click for full profile.' : 'All verified doctors. Click for full profile.'}
      </p>
      {list.length === 0 ? (
        <p className="admin-empty">No doctors.</p>
      ) : (
        <div className="admin-list-grid">
          {list.map((d) => (
            <Link key={d.id} to={`/admin/doctors/${d.id}`} className="admin-list-card">
              <div className="card-name">Dr. {d.fullName}</div>
              <p className="card-meta">
                {d.hospitalName}
                {d.qualification && ` · ${d.qualification}`}
                <br />
                {d.specialties?.length ? d.specialties.join(', ') : '—'}
                <br />
                {d.supportsInPerson && 'In-person'}
                {d.supportsInPerson && d.supportsVideo && ' · '}
                {d.supportsVideo && 'Video'}
                {d.phone && ` · 📞 ${d.phone}`}
              </p>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
