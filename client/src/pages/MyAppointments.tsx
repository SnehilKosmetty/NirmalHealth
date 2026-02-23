import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { api, type AppointmentDto } from '../api/client';

export default function MyAppointments() {
  const [list, setList] = useState<AppointmentDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api.appointments.my()
      .then(setList)
      .catch(() => setList([]))
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <div className="container page">Loading...</div>;

  return (
    <div className="container page">
      <h1 className="page-title">My Appointments</h1>
      {list.length === 0 ? (
        <p className="empty-msg">No appointments yet. <Link to="/hospitals">Find a hospital</Link> and book with a doctor.</p>
      ) : (
        <div className="appointment-list">
          {list.map((a) => (
            <div key={a.id} className="card" style={{ padding: '1rem', marginBottom: '1rem' }}>
              <strong>{a.doctorName}</strong> · {a.hospitalName}
              <p style={{ margin: '0.5rem 0 0', fontSize: '0.9rem', color: 'var(--color-text-muted)' }}>
                {a.date} at {a.startTime} · {a.consultationType} · {a.status}
              </p>
              {a.chiefComplaint && <p style={{ margin: '0.25rem 0 0', fontSize: '0.875rem' }}>Reason: {a.chiefComplaint}</p>}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
