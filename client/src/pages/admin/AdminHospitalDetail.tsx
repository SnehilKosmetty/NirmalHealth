import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { api, type HospitalDetail } from '../../api/client';
import './Admin.css';

export default function AdminHospitalDetail() {
  const { id } = useParams<{ id: string }>();
  const { user } = useAuth();
  const [hospital, setHospital] = useState<HospitalDetail | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!id) return;
    api.admin.hospitals.get(Number(id))
      .then(setHospital)
      .catch(() => setHospital(null))
      .finally(() => setLoading(false));
  }, [id]);

  if (loading) return <div className="container page admin-page admin-loading">Loading…</div>;
  if (!hospital) return <div className="container page admin-page"><p className="admin-empty">Hospital not found.</p><Link to="/admin/hospitals" className="admin-back-link">← Back to list</Link></div>;

  return (
    <div className="container page admin-page">
      <Link to="/admin/hospitals" className="admin-back-link">← Back to Hospitals</Link>
      <h1 className="page-title">{hospital.name}</h1>
      <p className="page-subtitle">Verified hospital details — for patient safety</p>

      <section className="admin-detail-section">
        <h3>Contact & location</h3>
        <div className="admin-detail-row"><span className="admin-detail-label">Address</span><span className="admin-detail-value">{hospital.address}</span></div>
        {hospital.area && <div className="admin-detail-row"><span className="admin-detail-label">Area</span><span className="admin-detail-value">{hospital.area}</span></div>}
        <div className="admin-detail-row"><span className="admin-detail-label">Phone</span><span className="admin-detail-value"><a href={`tel:${hospital.phone}`}>{hospital.phone}</a></span></div>
        {hospital.email && <div className="admin-detail-row"><span className="admin-detail-label">Email</span><span className="admin-detail-value"><a href={`mailto:${hospital.email}`}>{hospital.email}</a></span></div>}
        <div className="admin-detail-row"><span className="admin-detail-label">Beds</span><span className="admin-detail-value">{hospital.bedCount}</span></div>
        <div className="admin-detail-row"><span className="admin-detail-label">Type</span><span className="admin-detail-value">{hospital.type}</span></div>
        <div className="admin-detail-row"><span className="admin-detail-label">Emergency</span><span className="admin-detail-value">{hospital.isEmergency ? 'Yes' : 'No'}</span></div>
        {hospital.specialties?.length ? <div className="admin-detail-row"><span className="admin-detail-label">Specialties</span><span className="admin-detail-value">{hospital.specialties.join(', ')}</span></div> : null}
      </section>

      <section className="admin-detail-section">
        <h3>Doctors at this hospital</h3>
        {hospital.doctors?.length ? (
          <ul style={{ margin: 0, paddingLeft: '1.25rem' }}>
            {hospital.doctors.map((d) => (
              <li key={d.id} style={{ marginBottom: '0.5rem' }}>
                <Link to={`/admin/doctors/${d.id}`} style={{ fontWeight: 600 }}>{d.fullName}</Link>
                {d.qualification && ` · ${d.qualification}`}
                {d.specialties?.length ? ` · ${d.specialties.join(', ')}` : ''}
                {d.supportsInPerson && ' · In-person'}
                {d.supportsVideo && ' · Video'}
              </li>
            ))}
          </ul>
        ) : (
          <p className="admin-detail-value">No doctors listed yet.</p>
        )}
      </section>
    </div>
  );
}
