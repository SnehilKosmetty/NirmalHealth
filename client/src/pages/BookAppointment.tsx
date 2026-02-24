import { useEffect, useState, useMemo } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { api, type SlotDto } from '../api/client';
import { formatDateDMY, formatTime12h } from '../utils/dateTime';
import './BookAppointment.css';

export default function BookAppointment() {
  const { t } = useTranslation();
  const { doctorId } = useParams<{ doctorId: string }>();
  const navigate = useNavigate();
  const [slots, setSlots] = useState<SlotDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedSlotId, setSelectedSlotId] = useState<number | null>(null);
  const [chiefComplaint, setChiefComplaint] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const from = new Date();
  const to = new Date();
  to.setDate(to.getDate() + 14);
  const fromStr = from.toISOString().slice(0, 10);
  const toStr = to.toISOString().slice(0, 10);

  useEffect(() => {
    if (!doctorId) return;
    api.appointments.slots({
      doctorId: Number(doctorId),
      from: fromStr,
      to: toStr,
    })
      .then(setSlots)
      .catch(() => setSlots([]))
      .finally(() => setLoading(false));
  }, [doctorId, fromStr, toStr]);

  const slotsByDate = useMemo(() => {
    const map = new Map<string, SlotDto[]>();
    for (const s of slots) {
      const list = map.get(s.date) ?? [];
      list.push(s);
      map.set(s.date, list);
    }
    const sorted = Array.from(map.entries()).sort(([a], [b]) => a.localeCompare(b));
    return sorted;
  }, [slots]);

  const handleBook = async () => {
    if (!selectedSlotId) return;
    setSubmitting(true);
    try {
      await api.appointments.book({ slotId: selectedSlotId, chiefComplaint: chiefComplaint || undefined });
      navigate('/my-appointments');
    } finally {
      setSubmitting(false);
    }
  };

  if (!doctorId) return <div className="container page">{t('bookAppointment.invalidDoctor')}</div>;

  return (
    <div className="container page book-appointment">
      <h1 className="page-title">{t('bookAppointment.title')}</h1>
      <p className="page-subtitle">{t('bookAppointment.subtitle')}</p>

      <section className="section">
        <label className="section-label">{t('bookAppointment.chiefComplaintLabel')}</label>
        <input
          type="text"
          className="chief-complaint-input"
          value={chiefComplaint}
          onChange={(e) => setChiefComplaint(e.target.value)}
          placeholder={t('bookAppointment.chiefComplaintPlaceholder')}
        />
      </section>

      <section className="section">
        <span className="section-label">{t('bookAppointment.selectDateAndTime')}</span>
        {loading ? (
          <p className="loading-msg">{t('bookAppointment.loading')}</p>
        ) : slots.length === 0 ? (
          <p className="empty-msg">{t('bookAppointment.noSlots')}</p>
        ) : (
          <>
            {slotsByDate.map(([date, dateSlots]) => (
              <div key={date} className="date-group">
                <div className="date-group-label">{formatDateDMY(date)}</div>
                <div className="slot-grid">
                  {dateSlots.map((s) => (
                    <button
                      key={s.id}
                      type="button"
                      className={`slot-card ${selectedSlotId === s.id ? 'selected' : ''}`}
                      onClick={() => setSelectedSlotId(s.id)}
                    >
                      <div className="slot-time">
                        {formatTime12h(s.startTime)} – {formatTime12h(s.endTime)}
                      </div>
                      <div className="slot-type">{s.consultationType}</div>
                    </button>
                  ))}
                </div>
              </div>
            ))}
            <div className="confirm-row">
              <button
                type="button"
                className="btn btn-primary btn-confirm"
                disabled={!selectedSlotId || submitting}
                onClick={handleBook}
              >
                {submitting ? t('bookAppointment.booking') : t('bookAppointment.confirmBooking')}
              </button>
            </div>
          </>
        )}
      </section>
    </div>
  );
}
