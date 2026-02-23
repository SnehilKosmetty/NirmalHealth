import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Link, useSearchParams } from 'react-router-dom';
import { api, type HospitalListItem } from '../api/client';
import './Hospitals.css';

export default function Hospitals() {
  const { t } = useTranslation();
  const [searchParams] = useSearchParams();
  const specialtyFromUrl = searchParams.get('specialty') ?? '';
  const [list, setList] = useState<HospitalListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [specialty, setSpecialty] = useState(specialtyFromUrl);
  const [type, setType] = useState('');
  const [nearMe, setNearMe] = useState<HospitalListItem[] | null>(null);
  const [geoLoading, setGeoLoading] = useState(false);

  useEffect(() => {
    setSpecialty(specialtyFromUrl);
  }, [specialtyFromUrl]);

  useEffect(() => {
    let cancelled = false;
    api.hospitals.list({ search: search || undefined, specialty: specialty || undefined, type: type || undefined })
      .then((data) => { if (!cancelled) setList(data); })
      .catch(() => { if (!cancelled) setList([]); })
      .finally(() => { if (!cancelled) setLoading(false); });
    return () => { cancelled = true; };
  }, [search, specialty, type]);

  const findNearMe = () => {
    setGeoLoading(true);
    if (!navigator.geolocation) {
      api.hospitals.nearest({ count: 5, emergencyOnly: true }).then(setNearMe).finally(() => setGeoLoading(false));
      return;
    }
    navigator.geolocation.getCurrentPosition(
      (pos) => {
        api.hospitals.nearest({ lat: pos.coords.latitude, lon: pos.coords.longitude, count: 5, emergencyOnly: true })
          .then(setNearMe)
          .finally(() => setGeoLoading(false));
      },
      () => {
        api.hospitals.nearest({ count: 5, emergencyOnly: true }).then(setNearMe).finally(() => setGeoLoading(false));
      }
    );
  };

  const displayList = nearMe !== null ? nearMe : list;
  const typeLabel = (t: string) => {
    const key = t.toLowerCase().replace(/\s+/g, '');
    if (key === 'government') return 'hospitals.government';
    if (key === 'multispecialty') return 'hospitals.multiSpecialty';
    if (key === 'nursinghome') return 'hospitals.nursingHome';
    return 'hospitals.private';
  };

  return (
    <div className="container page hospitals-page">
      <h1 className="page-title">{t('hospitals.title')}</h1>
      <p className="page-subtitle">{t('hospitals.subtitle')}</p>

      <div className="hospitals-filters">
        <input
          type="search"
          className="search-input"
          placeholder={t('hospitals.searchPlaceholder')}
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <select value={specialty} onChange={(e) => setSpecialty(e.target.value)}>
          <option value="">{t('hospitals.allSpecialties')}</option>
          <option value="General Medicine">General Medicine</option>
          <option value="Emergency">Emergency</option>
          <option value="Pediatrics">Pediatrics</option>
          <option value="Cardiology">Cardiology</option>
          <option value="Dermatology">Dermatology</option>
          <option value="Gynecology">Gynecology</option>
          <option value="Orthopedics">Orthopedics</option>
        </select>
        <select value={type} onChange={(e) => setType(e.target.value)}>
          <option value="">{t('hospitals.allTypes')}</option>
          <option value="Government">{t('hospitals.government')}</option>
          <option value="Private">{t('hospitals.private')}</option>
          <option value="MultiSpecialty">{t('hospitals.multiSpecialty')}</option>
          <option value="NursingHome">{t('hospitals.nursingHome')}</option>
        </select>
        <button type="button" className="btn btn-primary" onClick={findNearMe} disabled={geoLoading}>
          📍 {t('hospitals.findNearMe')}
        </button>
        {nearMe !== null && (
          <button type="button" className="btn btn-outline" onClick={() => setNearMe(null)}>
            Show all
          </button>
        )}
      </div>

      {loading ? (
        <p className="loading-msg">Loading hospitals...</p>
      ) : (
        <div className="hospital-grid">
          {displayList.map((h) => (
            <div key={h.id} className="card hospital-card">
              <div className="hospital-card-image" />
              <div className="hospital-card-badge">{t(typeLabel(h.type))}</div>
              <div className="hospital-card-body">
                <h3>{h.name}</h3>
                <p className="hospital-address">📍 {h.address}</p>
                <p className="hospital-meta">🛏 {h.bedCount} {t('hospitals.beds')} · 📞 {h.phone}</p>
                <div className="hospital-specialties">
                  {h.specialties.slice(0, 4).map((s) => (
                    <span key={s} className="tag">{s}</span>
                  ))}
                  {h.specialties.length > 4 && <span className="tag">+{h.specialties.length - 4}</span>}
                </div>
                <div className="hospital-card-actions">
                  <Link to={`/hospitals/${h.id}`} className="btn btn-outline">{t('hospitals.viewDetails')}</Link>
                  <a href={`tel:${h.phone}`} className="btn btn-primary">📞 {t('hospitals.call')}</a>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
      {!loading && displayList.length === 0 && <p className="empty-msg">No hospitals found.</p>}
    </div>
  );
}
