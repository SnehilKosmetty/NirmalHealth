import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { api, type HospitalDetail as HospitalDetailType } from '../api/client';
import { useAuth } from '../context/AuthContext';

export default function HospitalDetail() {
  const { id } = useParams<{ id: string }>();
  const { t } = useTranslation();
  const { user } = useAuth();
  const [hospital, setHospital] = useState<HospitalDetailType | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!id) return;
    api.hospitals.get(Number(id))
      .then(setHospital)
      .catch(() => setHospital(null))
      .finally(() => setLoading(false));
  }, [id]);

  if (loading) return <div className="container page">{t('hospitalDetail.loading')}</div>;
  if (!hospital) return <div className="container page">{t('hospitalDetail.notFound')}</div>;

  return (
    <div className="container page">
      <h1 className="page-title">{hospital.name}</h1>
      <p className="page-subtitle">📍 {hospital.address}</p>
      <p><strong>{t('hospitalDetail.phone')}:</strong> <a href={`tel:${hospital.phone}`}>{hospital.phone}</a></p>
      {hospital.email && <p><strong>{t('hospitalDetail.email')}:</strong> {hospital.email}</p>}
      <p><strong>{t('hospitalDetail.beds')}:</strong> {hospital.bedCount} · <strong>{t('hospitalDetail.emergency')}:</strong> {hospital.isEmergency ? t('hospitalDetail.yes') : t('hospitalDetail.no')}</p>
      <div style={{ marginTop: '1rem' }}>
        {hospital.specialties.map((s) => <span key={s} className="tag" style={{ marginRight: 8 }}>{s}</span>)}
      </div>

      <h2 style={{ marginTop: '2rem', marginBottom: '1rem' }}>{t('hospitalDetail.doctors')}</h2>
      <div className="hospital-doctors-grid" style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(260px, 1fr))', gap: '1rem' }}>
        {hospital.doctors.map((d) => (
          <div key={d.id} className="card" style={{ padding: '1rem' }}>
            <h3 style={{ margin: '0 0 0.5rem' }}>{d.fullName}</h3>
            {d.qualification && <p style={{ margin: 0, fontSize: '0.875rem', color: 'var(--color-text-muted)' }}>{d.qualification}</p>}
            <div style={{ marginTop: '0.5rem' }}>
              {d.specialties.map((s) => <span key={s} className="tag" style={{ marginRight: 4, fontSize: '0.75rem' }}>{s}</span>)}
            </div>
            <p style={{ marginTop: '0.5rem', fontSize: '0.875rem' }}>
              {d.supportsInPerson && t('hospitalDetail.inPerson') + ' · '}{d.supportsVideo && t('hospitalDetail.video')}
            </p>
            {user && (
              <Link to={`/book/${d.id}`} className="btn btn-primary" style={{ marginTop: '0.75rem' }}>{t('hospitalDetail.bookAppointment')}</Link>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
