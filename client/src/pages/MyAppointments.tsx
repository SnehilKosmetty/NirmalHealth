import { useEffect, useState, useMemo } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { api, type AppointmentDto, type SlotDto } from '../api/client';
import { formatDateDMY, formatTime12h } from '../utils/dateTime';
import './MyAppointments.css';

function statusClass(status: string): string {
  const s = (status || '').toLowerCase();
  if (s.includes('confirm')) return 'confirmed';
  if (s.includes('complete')) return 'completed';
  if (s.includes('cancel')) return 'cancelled';
  if (s.includes('pending')) return 'pending';
  return 'pending';
}

function canEditOrCancel(status: string): boolean {
  const s = (status || '').toLowerCase();
  return s === 'scheduled';
}

const from = new Date();
const to = new Date();
to.setDate(to.getDate() + 14);
const fromStr = from.toISOString().slice(0, 10);
const toStr = to.toISOString().slice(0, 10);

export default function MyAppointments() {
  const { t } = useTranslation();
  const [list, setList] = useState<AppointmentDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [editComplaint, setEditComplaint] = useState('');
  const [selectedSlotId, setSelectedSlotId] = useState<number | null>(null);
  const [actionLoading, setActionLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [rescheduleSlots, setRescheduleSlots] = useState<SlotDto[]>([]);
  const [rescheduleSlotsLoading, setRescheduleSlotsLoading] = useState(false);

  const editingAppointment = list.find((a) => a.id === editingId);

  const refresh = () => {
    setError(null);
    api.appointments.my()
      .then((data) => {
        setList(data);
        setSuccess(null);
      })
      .catch(() => setList([]));
  };

  useEffect(() => {
    api.appointments.my()
      .then(setList)
      .catch(() => setList([]))
      .finally(() => setLoading(false));
  }, []);

  useEffect(() => {
    if (!editingAppointment?.doctorId) {
      setRescheduleSlots([]);
      return;
    }
    setRescheduleSlotsLoading(true);
    api.appointments.slots({
      doctorId: editingAppointment.doctorId,
      from: fromStr,
      to: toStr,
    })
      .then(setRescheduleSlots)
      .catch(() => setRescheduleSlots([]))
      .finally(() => setRescheduleSlotsLoading(false));
  }, [editingAppointment?.doctorId]);

  const slotsByDate = useMemo(() => {
    const map = new Map<string, SlotDto[]>();
    for (const s of rescheduleSlots) {
      const list = map.get(s.date) ?? [];
      list.push(s);
      map.set(s.date, list);
    }
    return Array.from(map.entries()).sort(([a], [b]) => a.localeCompare(b));
  }, [rescheduleSlots]);

  const handleCancel = async (a: AppointmentDto) => {
    if (!canEditOrCancel(a.status)) return;
    if (!window.confirm(t('myAppointments.cancelConfirm'))) return;
    setError(null);
    setActionLoading(true);
    try {
      await api.appointments.cancel(a.id);
      setSuccess(t('myAppointments.cancelSuccess'));
      refresh();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to cancel appointment.');
    } finally {
      setActionLoading(false);
    }
  };

  const startEdit = (a: AppointmentDto) => {
    if (!canEditOrCancel(a.status)) return;
    setEditingId(a.id);
    setEditComplaint(a.chiefComplaint ?? '');
    setSelectedSlotId(null);
    setError(null);
    setSuccess(null);
  };

  const handleSaveEdit = async () => {
    if (editingId == null) return;
    setError(null);
    setActionLoading(true);
    try {
      const body: { chiefComplaint?: string; slotId?: number } = {
        chiefComplaint: editComplaint || undefined,
      };
      if (selectedSlotId != null && selectedSlotId !== editingAppointment?.slotId) {
        body.slotId = selectedSlotId;
      }
      await api.appointments.update(editingId, body);
      setSuccess(t('myAppointments.saveSuccess'));
      setEditingId(null);
      setSelectedSlotId(null);
      refresh();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to save.');
    } finally {
      setActionLoading(false);
    }
  };

  return (
    <div className="container page my-appointments">
      <h1 className="page-title">{t('myAppointments.title')}</h1>
      <p className="page-subtitle">{t('myAppointments.subtitle')}</p>

      {error && (
        <div className="appointment-error" role="alert">
          {error}
        </div>
      )}
      {success && (
        <div className="appointment-success" role="status">
          {success}
        </div>
      )}

      {loading ? (
        <p className="loading-msg">{t('myAppointments.loading')}</p>
      ) : list.length === 0 ? (
        <p className="empty-msg">
          {t('myAppointments.empty')} <Link to="/hospitals">{t('myAppointments.findHospital')}</Link> {t('myAppointments.findHospitalAndBook')}
        </p>
      ) : (
        <div className="appointment-list">
          {list.map((a) => (
            <article key={a.id} className="appointment-card card">
              <div className="card-doctor">{a.doctorName}</div>
              <div className="card-hospital">{a.hospitalName}</div>
              <div className="card-meta">
                <span className="card-date-time">
                  {formatDateDMY(a.date)} at {formatTime12h(a.startTime)}
                </span>
                <span className="card-type">· {a.consultationType}</span>
                <span className={`status-badge ${statusClass(a.status)}`}>{a.status}</span>
              </div>
              {a.chiefComplaint && editingId !== a.id && (
                <div className="chief-complaint">
                  <strong>{t('myAppointments.reason')}:</strong> {a.chiefComplaint}
                </div>
              )}
              {editingId === a.id ? (
                <div className="card-edit-form">
                  <label>
                    <span className="edit-label">{t('myAppointments.editReason')}</span>
                    <input
                      type="text"
                      className="edit-input"
                      value={editComplaint}
                      onChange={(e) => setEditComplaint(e.target.value)}
                      placeholder={t('bookAppointment.chiefComplaintPlaceholder')}
                    />
                  </label>
                  <div className="edit-section">
                    <span className="edit-label">{t('myAppointments.changeDateTime')}</span>
                    <p className="edit-hint">{t('myAppointments.currentSlot')}: {formatDateDMY(a.date)} at {formatTime12h(a.startTime)}</p>
                    {rescheduleSlotsLoading ? (
                      <p className="slots-loading">{t('myAppointments.loadingSlots')}</p>
                    ) : (
                      <div className="reschedule-slot-grid">
                        {slotsByDate.map(([date, dateSlots]) => (
                          <div key={date} className="reschedule-date-group">
                            <div className="reschedule-date-label">{formatDateDMY(date)}</div>
                            <div className="reschedule-slot-list">
                              {dateSlots.map((s) => (
                                <button
                                  key={s.id}
                                  type="button"
                                  className={`reschedule-slot-btn ${selectedSlotId === s.id ? 'selected' : ''}`}
                                  onClick={() => setSelectedSlotId(s.id)}
                                >
                                  {formatTime12h(s.startTime)} – {formatTime12h(s.endTime)}
                                </button>
                              ))}
                            </div>
                          </div>
                        ))}
                      </div>
                    )}
                  </div>
                  <div className="card-actions">
                    <button type="button" className="btn btn-primary" onClick={handleSaveEdit} disabled={actionLoading}>
                      {actionLoading ? t('myAppointments.saving') : t('myAppointments.save')}
                    </button>
                    <button type="button" className="btn btn-outline" onClick={() => { setEditingId(null); setSelectedSlotId(null); }} disabled={actionLoading}>
                      {t('myAppointments.cancelAction')}
                    </button>
                  </div>
                </div>
              ) : (
                canEditOrCancel(a.status) && (
                  <div className="card-actions">
                    <button type="button" className="btn btn-outline btn-sm" onClick={() => startEdit(a)} disabled={actionLoading}>
                      {t('myAppointments.edit')}
                    </button>
                    <button type="button" className="btn btn-outline btn-sm btn-danger-outline" onClick={() => handleCancel(a)} disabled={actionLoading}>
                      {t('myAppointments.cancelAppointment')}
                    </button>
                  </div>
                )
              )}
            </article>
          ))}
        </div>
      )}
    </div>
  );
}
