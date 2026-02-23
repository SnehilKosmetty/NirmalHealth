import { useEffect, useState } from 'react';
import { useAuth } from '../../context/AuthContext';
import { api, type AppointmentDto } from '../../api/client';

export default function AdminAppointments() {
  const { user } = useAuth();
  const [list, setList] = useState<AppointmentDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [hospitalId, setHospitalId] = useState(user?.hospitalId ?? 1);
  const [date, setDate] = useState('');

  useEffect(() => {
    const hid = user?.roles.includes('SuperAdmin') ? hospitalId : (user?.hospitalId ?? 0);
    if (!hid) {
      setLoading(false);
      return;
    }
    api.admin.appointments.list(hid, date || undefined)
      .then(setList)
      .catch(() => setList([]))
      .finally(() => setLoading(false));
  }, [user?.hospitalId, user?.roles, hospitalId, date]);

  const showHospitalFilter = user?.roles.includes('SuperAdmin');

  return (
    <div className="container page">
      <h1 className="page-title">Admin · Appointments</h1>
      <p className="page-subtitle">Bookings for your hospital</p>
      {showHospitalFilter && (
        <div style={{ marginBottom: '1rem' }}>
          <label>
            Hospital ID{' '}
            <input
              type="number"
              min={1}
              value={hospitalId}
              onChange={(e) => setHospitalId(Number(e.target.value))}
              style={{ width: 80, padding: '0.35rem' }}
            />
          </label>
          <label style={{ marginLeft: '1rem' }}>
            Date (optional){' '}
            <input
              type="date"
              value={date}
              onChange={(e) => setDate(e.target.value)}
              style={{ padding: '0.35rem' }}
            />
          </label>
        </div>
      )}
      {!showHospitalFilter && (
        <label style={{ marginBottom: '1rem', display: 'block' }}>
          Date (optional){' '}
          <input type="date" value={date} onChange={(e) => setDate(e.target.value)} style={{ padding: '0.35rem' }} />
        </label>
      )}
      {loading ? (
        <p>Loading...</p>
      ) : (
        <div className="admin-list">
          {list.map((a) => (
            <div key={a.id} className="card" style={{ padding: '1rem', marginBottom: '0.75rem' }}>
              <strong>{a.patientName}</strong> with {a.doctorName} at {a.hospitalName}
              <p style={{ margin: '0.25rem 0 0', fontSize: '0.9rem', color: 'var(--color-text-muted)' }}>
                {a.date} {a.startTime} · {a.consultationType} · {a.status}
              </p>
            </div>
          ))}
        </div>
      )}
      {!loading && list.length === 0 && <p className="empty-msg">No appointments.</p>}
    </div>
  );
}
