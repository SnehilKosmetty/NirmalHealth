import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { api, type DoctorDetail } from '../../api/client';
import './Admin.css';

export default function AdminDoctorDetail() {
  const { id } = useParams<{ id: string }>();
  const { user } = useAuth();
  const [doctor, setDoctor] = useState<DoctorDetail | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!id) return;
    api.admin.doctors.get(Number(id))
      .then(setDoctor)
      .catch(() => setDoctor(null))
      .finally(() => setLoading(false));
  }, [id]);

  if (loading) return <div className="container page admin-page admin-loading">Loading…</div>;
  if (!doctor) return <div className="container page admin-page"><p className="admin-empty">Doctor not found.</p><Link to="/admin/doctors" className="admin-back-link">← Back to list</Link></div>;

  return (
    <div className="container page admin-page">
      <Link to="/admin/doctors" className="admin-back-link">← Back to Doctors</Link>
      <h1 className="page-title">Dr. {doctor.fullName}</h1>
      <p className="page-subtitle">Verified doctor profile — for patient safety</p>

      <section className="admin-detail-section">
        <h3>Profile & qualification</h3>
        <div className="admin-detail-row"><span className="admin-detail-label">Hospital</span><span className="admin-detail-value"><Link to={`/admin/hospitals/${doctor.hospitalId}`}>{doctor.hospitalName}</Link></span></div>
        {doctor.qualification && <div className="admin-detail-row"><span className="admin-detail-label">Qualification</span><span className="admin-detail-value">{doctor.qualification}</span></div>}
        {doctor.registrationNumber && <div className="admin-detail-row"><span className="admin-detail-label">Registration no.</span><span className="admin-detail-value">{doctor.registrationNumber}</span></div>}
        {doctor.specialties?.length ? <div className="admin-detail-row"><span className="admin-detail-label">Specialties</span><span className="admin-detail-value">{doctor.specialties.join(', ')}</span></div> : null}
        <div className="admin-detail-row"><span className="admin-detail-label">Consultation</span><span className="admin-detail-value">{doctor.supportsInPerson && 'In-person'}{doctor.supportsInPerson && doctor.supportsVideo && ' · '}{doctor.supportsVideo && 'Video'}</span></div>
      </section>

      <section className="admin-detail-section">
        <h3>Contact</h3>
        {doctor.phone && <div className="admin-detail-row"><span className="admin-detail-label">Phone</span><span className="admin-detail-value"><a href={`tel:${doctor.phone}`}>{doctor.phone}</a></span></div>}
        {doctor.email && <div className="admin-detail-row"><span className="admin-detail-label">Email</span><span className="admin-detail-value"><a href={`mailto:${doctor.email}`}>{doctor.email}</a></span></div>}
        {!doctor.phone && !doctor.email && <p className="admin-detail-value">No contact details on file.</p>}
      </section>
    </div>
  );
}
