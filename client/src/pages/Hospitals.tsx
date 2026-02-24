import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Link, useSearchParams } from 'react-router-dom';
import { api, type HospitalListItem } from '../api/client';
import './Hospitals.css';

function SearchIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden>
      <circle cx="11" cy="11" r="8" />
      <path d="m21 21-4.35-4.35" />
    </svg>
  );
}
function FilterIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden>
      <polygon points="22 3 2 3 10 12.46 10 19 14 21 14 12.46 22 3" />
    </svg>
  );
}
function BuildingIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden>
      <rect x="4" y="2" width="16" height="20" rx="2" ry="2" />
      <path d="M9 22v-4h6v4" />
      <path d="M8 6h.01" /><path d="M16 6h.01" /><path d="M12 6h.01" /><path d="M12 10h.01" /><path d="M12 14h.01" /><path d="M16 10h.01" /><path d="M16 14h.01" /><path d="M8 10h.01" /><path d="M8 14h.01" />
    </svg>
  );
}
function LocationIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden>
      <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z" />
      <circle cx="12" cy="10" r="3" />
    </svg>
  );
}
function StarIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="currentColor" stroke="none" aria-hidden>
      <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z" />
    </svg>
  );
}
function BedIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden>
      <path d="M2 4v16" /><path d="M2 8h18a2 2 0 0 1 2 2v10" /><path d="M2 17h20" /><path d="M6 8V4a2 2 0 0 1 2-2h8a2 2 0 0 1 2 2v4" />
    </svg>
  );
}
function PhoneIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden>
      <path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.84 12.84 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.7A2 2 0 0 1 22 16.92z" />
    </svg>
  );
}

export default function Hospitals() {
  const { t } = useTranslation();
  const [searchParams] = useSearchParams();
  const specialtyFromUrl = searchParams.get('specialty') ?? '';
  const [list, setList] = useState<HospitalListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [specialty, setSpecialty] = useState(specialtyFromUrl);
  const [type, setType] = useState('');
  const [applied, setApplied] = useState({ search: '', specialty: specialtyFromUrl, type: '' });
  const [nearMe, setNearMe] = useState<HospitalListItem[] | null>(null);
  const [geoLoading, setGeoLoading] = useState(false);

  useEffect(() => {
    setSpecialty(specialtyFromUrl);
    setApplied((a) => ({ ...a, specialty: specialtyFromUrl }));
  }, [specialtyFromUrl]);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    api.hospitals.list({
      search: applied.search || undefined,
      specialty: applied.specialty || undefined,
      type: applied.type || undefined,
    })
      .then((data) => { if (!cancelled) setList(data); })
      .catch(() => { if (!cancelled) setList([]); })
      .finally(() => { if (!cancelled) setLoading(false); });
    return () => { cancelled = true; };
  }, [applied.search, applied.specialty, applied.type]);

  const handleApply = () => {
    setNearMe(null);
    setApplied({ search: search.trim(), specialty, type });
  };

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
  const typeLabel = (typ: string) => {
    const key = typ.toLowerCase().replace(/\s+/g, '');
    if (key === 'government') return t('hospitals.government');
    if (key === 'multispecialty') return t('hospitals.multiSpecialty');
    if (key === 'nursinghome') return t('hospitals.nursingHome');
    return t('hospitals.private');
  };
  const isGovernment = (typ: string) => typ.toLowerCase() === 'government';

  return (
    <div className="container page hospitals-page">
      <h1 className="page-title">{t('hospitals.title')}</h1>
      <p className="page-subtitle">{t('hospitals.subtitle')}</p>

      <div className="hospitals-filters-row">
        <div className="search-wrap">
          <SearchIcon className="search-icon" aria-hidden />
          <input
            type="search"
            className="search-input"
            placeholder={t('hospitals.searchPlaceholder')}
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && handleApply()}
          />
        </div>
        <div className="filter-select-wrap">
          <FilterIcon className="filter-select-icon" aria-hidden />
          <select value={specialty} onChange={(e) => setSpecialty(e.target.value)} aria-label={t('hospitals.allSpecialties')}>
            <option value="">{t('hospitals.allSpecialties')}</option>
            <option value="General Medicine">General Medicine</option>
            <option value="Emergency">Emergency</option>
            <option value="Pediatrics">Pediatrics</option>
            <option value="Cardiology">Cardiology</option>
            <option value="Dermatology">Dermatology</option>
            <option value="Gynecology">Gynecology</option>
            <option value="Orthopedics">Orthopedics</option>
          </select>
        </div>
        <div className="filter-select-wrap">
          <BuildingIcon className="filter-select-icon" aria-hidden />
          <select value={type} onChange={(e) => setType(e.target.value)} aria-label={t('hospitals.allTypes')}>
            <option value="">{t('hospitals.allTypes')}</option>
            <option value="Government">{t('hospitals.government')}</option>
            <option value="Private">{t('hospitals.private')}</option>
            <option value="MultiSpecialty">{t('hospitals.multiSpecialty')}</option>
            <option value="NursingHome">{t('hospitals.nursingHome')}</option>
          </select>
        </div>
        <button type="button" className="btn btn-primary hospitals-apply-btn" onClick={handleApply}>
          {t('hospitals.apply')}
        </button>
      </div>

      <div className="hospitals-near-me-wrap">
        <button type="button" className="btn btn-outline hospitals-near-me-btn" onClick={findNearMe} disabled={geoLoading}>
          <LocationIcon className="hospitals-near-me-icon" aria-hidden />
          {t('hospitals.findNearMe')}
        </button>
        {nearMe !== null && (
          <button type="button" className="btn btn-outline hospitals-show-all" onClick={() => setNearMe(null)}>
            Show all
          </button>
        )}
      </div>

      {loading ? (
        <p className="loading-msg">Loading hospitals...</p>
      ) : (
        <div className="hospital-grid">
          {displayList.map((h) => (
            <article key={h.id} className="card hospital-card">
              <div className="hospital-card-image-wrap">
                <div className="hospital-card-image" />
                {isGovernment(h.type) && (
                  <span className="hospital-card-badge hospital-card-badge-government">{typeLabel(h.type)}</span>
                )}
              </div>
              <div className="hospital-card-body">
                <h3 className="hospital-card-name">{h.name}</h3>
                <p className="hospital-address">
                  <LocationIcon className="hospital-meta-icon" aria-hidden />
                  {h.address}
                </p>
                <p className="hospital-rating">
                  <StarIcon className="hospital-star-icon" aria-hidden />
                  <span>—</span>
                </p>
                <div className="hospital-specialties">
                  {h.specialties.slice(0, 3).map((s) => (
                    <span key={s} className="hospital-specialty-tag">{s}</span>
                  ))}
                  {h.specialties.length > 3 && (
                    <span className="hospital-specialty-tag hospital-specialty-more">+{h.specialties.length - 3}</span>
                  )}
                </div>
                <p className="hospital-meta">
                  <BedIcon className="hospital-meta-icon" aria-hidden />
                  {h.bedCount} {t('hospitals.beds')}
                </p>
                <p className="hospital-meta">
                  <PhoneIcon className="hospital-meta-icon" aria-hidden />
                  {h.phone}
                </p>
                <div className="hospital-card-actions">
                  <Link to={`/hospitals/${h.id}`} className="btn btn-outline hospital-btn-view">
                    {t('hospitals.viewDetails')}
                  </Link>
                  <a href={`tel:${h.phone}`} className="btn btn-primary hospital-btn-call">
                    <PhoneIcon className="hospital-btn-call-icon" aria-hidden />
                    {t('hospitals.call')}
                  </a>
                </div>
              </div>
            </article>
          ))}
        </div>
      )}
      {!loading && displayList.length === 0 && <p className="empty-msg">No hospitals found.</p>}
    </div>
  );
}
