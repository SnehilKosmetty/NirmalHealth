import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { api, type EmergencyContact, type HospitalListItem } from '../api/client';
import './Emergency.css';

export default function Emergency() {
  const { t } = useTranslation();
  const [contacts, setContacts] = useState<EmergencyContact[]>([]);
  const [hospitals, setHospitals] = useState<HospitalListItem[]>([]);
  const [loadingHospitals, setLoadingHospitals] = useState(false);

  useEffect(() => {
    api.emergency.contacts().then(setContacts).catch(() => setContacts([]));
  }, []);

  const findNearest = () => {
    setLoadingHospitals(true);
    if (!navigator.geolocation) {
      api.emergency.nearestHospitals({ count: 5 }).then(setHospitals).finally(() => setLoadingHospitals(false));
      return;
    }
    navigator.geolocation.getCurrentPosition(
      (pos) => {
        api.emergency.nearestHospitals({
          lat: pos.coords.latitude,
          lon: pos.coords.longitude,
          count: 5,
        }).then(setHospitals).finally(() => setLoadingHospitals(false));
      },
      () => {
        api.emergency.nearestHospitals({ count: 5 }).then(setHospitals).finally(() => setLoadingHospitals(false));
      }
    );
  };

  const contactLabels: Record<string, string> = {
    'Ambulance': t('emergency.ambulance'),
    'Police': t('emergency.police'),
    'Fire': t('emergency.fire'),
    'Women Helpline': t('emergency.womenHelpline'),
    'Child Helpline': t('emergency.childHelpline'),
  };

  return (
    <div className="emergency-page">
      <section className="emergency-banner">
        <span className="emergency-icon">⚠</span>
        <h1>{t('emergency.title')}</h1>
        <p>{t('emergency.subtitle')}</p>
      </section>

      <div className="container">
        <h2 className="section-heading">{t('emergency.contacts')}</h2>
        <div className="emergency-contacts">
          {contacts.map((c) => (
            <a key={c.number} href={`tel:${c.number}`} className="emergency-contact-card">
              <span className="contact-number">{c.number}</span>
              <span className="contact-name">{contactLabels[c.name] || c.name}</span>
              <span className="contact-desc">{c.description}</span>
            </a>
          ))}
        </div>

        <button type="button" className="btn btn-emergency btn-large" onClick={findNearest} disabled={loadingHospitals}>
          📍 {t('emergency.findNearest')}
        </button>

        {hospitals.length > 0 && (
          <>
            <h2 className="section-heading">📍 {t('emergency.nearestHospitals')}</h2>
            <div className="nearest-hospitals">
              {hospitals.map((h) => (
                <div key={h.id} className="card hospital-row">
                  <div>
                    <strong>{h.name}</strong>
                    <p className="hospital-address">{h.address}</p>
                  </div>
                  <a href={`tel:${h.phone}`} className="btn btn-danger">
                    📞 {t('emergency.callHospital')} {h.phone}
                  </a>
                </div>
              ))}
            </div>
          </>
        )}
      </div>
    </div>
  );
}
