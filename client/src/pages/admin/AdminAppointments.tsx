import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { api, type AppointmentDto, type HospitalListItem } from '../../api/client';
import { formatDateDMY, formatTime12h } from '../../utils/dateTime';
import './Admin.css';

export default function AdminAppointments() {
  const { user } = useAuth();
  const [list, setList] = useState<AppointmentDto[]>([]);
  const [hospitals, setHospitals] = useState<HospitalListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const isSuperAdmin = user?.roles.includes('SuperAdmin');
  const hospitalIdFromUser = user?.hospitalId ?? null;
  const [hospitalId, setHospitalId] = useState<number | ''>(isSuperAdmin ? '' : (hospitalIdFromUser ?? ''));
  const [date, setDate] = useState('');

  useEffect(() => {
    if (isSuperAdmin) {
      api.admin.hospitals.list().then(setHospitals).catch(() => setHospitals([]));
    }
  }, [isSuperAdmin]);

  useEffect(() => {
    if (!isSuperAdmin && !hospitalIdFromUser) {
      setLoading(false);
      return;
    }
    const hid = isSuperAdmin ? (hospitalId === '' ? undefined : Number(hospitalId)) : hospitalIdFromUser ?? undefined;
    api.admin.appointments.list(hid ?? null, date || undefined)
      .then(setList)
      .catch(() => setList([]))
      .finally(() => setLoading(false));
  }, [isSuperAdmin, hospitalIdFromUser, hospitalId, date]);

  if (!isSuperAdmin && hospitalIdFromUser == null) {
    return (
      <div className="container page admin-page">
        <Link to="/admin" className="admin-back-link">← Back to Dashboard</Link>
        <h1 className="page-title">Appointments</h1>
        <p className="admin-empty">No hospital assigned to your account. Contact Super Admin.</p>
      </div>
    );
  }

  return (
    <div className="container page admin-page">
      <Link to="/admin" className="admin-back-link">← Back to Dashboard</Link>
      <h1 className="page-title">Appointments</h1>
      <p className="page-subtitle">
        {isSuperAdmin ? 'View all bookings or filter by hospital' : 'Bookings for your hospital'}
      </p>

      {isSuperAdmin && (
        <div className="admin-filters">
          <label>
            Hospital
            <select
              value={hospitalId === '' ? 'all' : hospitalId}
              onChange={(e) => setHospitalId(e.target.value === 'all' ? '' : Number(e.target.value))}
            >
              <option value="all">All hospitals</option>
              {hospitals.map((h) => (
                <option key={h.id} value={h.id}>{h.name}</option>
              ))}
            </select>
          </label>
          <label>
            Date (optional)
            <input type="date" value={date} onChange={(e) => setDate(e.target.value)} />
          </label>
        </div>
      )}
      {!isSuperAdmin && (
        <div className="admin-filters">
          <label>
            Date (optional)
            <input type="date" value={date} onChange={(e) => setDate(e.target.value)} />
          </label>
        </div>
      )}

      {loading ? (
        <p className="admin-loading">Loading…</p>
      ) : list.length === 0 ? (
        <p className="admin-empty">No appointments.</p>
      ) : (
        <div className="admin-list-grid">
          {list.map((a) => (
            <div key={a.id} className="admin-list-card" style={{ cursor: 'default', textDecoration: 'none' }}>
              <div className="card-name">{a.patientName}</div>
              <p className="card-meta">
                with Dr. {a.doctorName} at {a.hospitalName}
                <br />
                {formatDateDMY(a.date)} at {formatTime12h(a.startTime)}
                <br />
                {a.consultationType} · <strong>{a.status}</strong>
                {a.chiefComplaint && <><br />Reason: {a.chiefComplaint}</>}
              </p>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
