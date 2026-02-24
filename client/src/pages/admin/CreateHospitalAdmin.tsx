import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { api, type HospitalListItem } from '../../api/client';
import './Admin.css';

export default function CreateHospitalAdmin() {
  const { user } = useAuth();
  const [hospitals, setHospitals] = useState<HospitalListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [message, setMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);
  const [form, setForm] = useState({
    email: '',
    phone: '',
    password: '',
    fullName: '',
    hospitalId: 1,
  });

  useEffect(() => {
    api.admin.hospitals.list()
      .then(setHospitals)
      .catch(() => setHospitals([]))
      .finally(() => setLoading(false));
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setMessage(null);
    setSubmitting(true);
    try {
      await api.admin.users.createHospitalAdmin({
        ...form,
        preferredLanguage: localStorage.getItem('lang') || 'en',
      });
      setMessage({ type: 'success', text: 'Hospital admin created. They can log in with the email and password you set.' });
      setForm({ email: '', phone: '', password: '', fullName: '', hospitalId: hospitals[0]?.id ?? 1 });
    } catch (err: unknown) {
      const text = err && typeof err === 'object' && 'message' in err ? String((err as { message: string }).message) : 'Failed to create hospital admin.';
      setMessage({ type: 'error', text });
    } finally {
      setSubmitting(false);
    }
  };

  if (!user?.roles.includes('SuperAdmin')) {
    return (
      <div className="container page admin-page">
        <Link to="/admin" className="admin-back-link">← Back to Dashboard</Link>
        <p className="admin-empty">Only Super Admin can create hospital admins.</p>
      </div>
    );
  }

  if (loading) return <div className="container page admin-page admin-loading">Loading hospitals…</div>;

  return (
    <div className="container page admin-page">
      <Link to="/admin" className="admin-back-link">← Back to Dashboard</Link>
      <h1 className="page-title">Create Hospital Admin</h1>
      <p className="page-subtitle">
        Create credentials for a hospital admin and assign them to one hospital. They will only see that hospital&apos;s doctors and appointments. Keep credentials secure.
      </p>
      <form onSubmit={handleSubmit} className="admin-form">
        <div className="form-group">
          <label className="form-label">Full name</label>
          <input
            type="text"
            className="form-input"
            required
            value={form.fullName}
            onChange={(e) => setForm((f) => ({ ...f, fullName: e.target.value }))}
          />
        </div>
        <div className="form-group">
          <label className="form-label">Email</label>
          <input
            type="email"
            className="form-input"
            required
            value={form.email}
            onChange={(e) => setForm((f) => ({ ...f, email: e.target.value }))}
          />
        </div>
        <div className="form-group">
          <label className="form-label">Phone</label>
          <input
            type="tel"
            className="form-input"
            required
            value={form.phone}
            onChange={(e) => setForm((f) => ({ ...f, phone: e.target.value }))}
          />
        </div>
        <div className="form-group">
          <label className="form-label">Password</label>
          <input
            type="password"
            className="form-input"
            required
            minLength={6}
            value={form.password}
            onChange={(e) => setForm((f) => ({ ...f, password: e.target.value }))}
          />
        </div>
        <div className="form-group">
          <label className="form-label">Assign to hospital</label>
          <select
            className="form-select"
            value={form.hospitalId}
            onChange={(e) => setForm((f) => ({ ...f, hospitalId: Number(e.target.value) }))}
          >
            {hospitals.map((h) => (
              <option key={h.id} value={h.id}>{h.name}</option>
            ))}
          </select>
        </div>
        {message && (
          <div className={`message ${message.type}`}>{message.text}</div>
        )}
        <div className="form-actions">
          <button type="submit" className="btn btn-primary" disabled={submitting}>
            {submitting ? 'Creating…' : 'Create Hospital Admin'}
          </button>
          <Link to="/admin" className="btn btn-outline">Cancel</Link>
        </div>
      </form>
    </div>
  );
}
