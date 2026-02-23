import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { api, type SlotDto } from '../api/client';

export default function BookAppointment() {
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

  if (!doctorId) return <div className="container page">Invalid doctor.</div>;
  if (loading) return <div className="container page">Loading slots...</div>;

  return (
    <div className="container page">
      <h1 className="page-title">Book appointment</h1>
      <p className="page-subtitle">Select a slot (next 14 days)</p>

      <label style={{ display: 'block', marginBottom: '1rem' }}>
        <span>Chief complaint (optional)</span>
        <input
          type="text"
          value={chiefComplaint}
          onChange={(e) => setChiefComplaint(e.target.value)}
          placeholder="Brief reason for visit"
          style={{ width: '100%', padding: '0.5rem', marginTop: '0.25rem' }}
        />
      </label>

      <div className="slot-grid" style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(200px, 1fr))', gap: '0.75rem', marginBottom: '1.5rem' }}>
        {slots.map((s) => (
          <button
            key={s.id}
            type="button"
            className={`card ${selectedSlotId === s.id ? 'selected' : ''}`}
            style={{
              padding: '0.75rem',
              textAlign: 'left',
              border: selectedSlotId === s.id ? '2px solid var(--color-primary)' : '1px solid var(--color-border)',
            }}
            onClick={() => setSelectedSlotId(s.id)}
          >
            <strong>{s.date}</strong><br />
            {s.startTime} – {s.endTime}<br />
            <small>{s.consultationType}</small>
          </button>
        ))}
      </div>
      {slots.length === 0 && <p className="empty-msg">No available slots in this period.</p>}

      <button
        type="button"
        className="btn btn-primary"
        disabled={!selectedSlotId || submitting}
        onClick={handleBook}
      >
        {submitting ? 'Booking...' : 'Confirm booking'}
      </button>
    </div>
  );
}
