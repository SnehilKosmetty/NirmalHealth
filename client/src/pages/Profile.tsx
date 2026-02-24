import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../context/AuthContext';
import { api } from '../api/client';
import './Profile.css';

const LANGUAGES = [
  { value: 'en', label: 'English' },
  { value: 'hi', label: 'हिंदी' },
  { value: 'te', label: 'తెలుగు' },
];

export default function Profile() {
  const { t, i18n } = useTranslation();
  const { user, refreshUser } = useAuth();
  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [phone, setPhone] = useState('');
  const [aadhaarNumber, setAadhaarNumber] = useState('');
  const [preferredLanguage, setPreferredLanguage] = useState('en');
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);

  useEffect(() => {
    if (user) {
      setFullName(user.fullName);
      setEmail(user.email);
      setPhone(user.phone);
      setPreferredLanguage(user.preferredLanguage || 'en');
    }
  }, [user]);

  const handleVerifyAadhaar = async (e: React.FormEvent) => {
    e.preventDefault();
    const digits = aadhaarNumber.replace(/\D/g, '');
    if (digits.length !== 12) {
      setMessage({ type: 'error', text: t('register.aadhaarRequired') });
      return;
    }
    setMessage(null);
    setLoading(true);
    try {
      await api.auth.updateProfile({ aadhaarNumber: digits });
      await refreshUser();
      setMessage({ type: 'success', text: t('profile.saved') });
      setAadhaarNumber('');
    } catch (err) {
      setMessage({ type: 'error', text: err instanceof Error ? err.message : 'Verification failed.' });
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setMessage(null);
    setLoading(true);
    try {
      const body: { fullName?: string; email?: string; phone?: string; aadhaarNumber?: string; preferredLanguage?: string } = {
        fullName: fullName.trim() || undefined,
        email: email.trim() || undefined,
        phone: phone.trim() || undefined,
        preferredLanguage: preferredLanguage || undefined,
      };
      if (aadhaarNumber.trim()) {
        const digits = aadhaarNumber.replace(/\D/g, '');
        if (digits.length === 12) body.aadhaarNumber = digits;
      }
      await api.auth.updateProfile(body);
      await refreshUser();
      if (body.preferredLanguage) {
        localStorage.setItem('lang', body.preferredLanguage);
        i18n.changeLanguage(body.preferredLanguage);
      }
      setMessage({ type: 'success', text: t('profile.saved') });
      setAadhaarNumber('');
    } catch (err) {
      setMessage({ type: 'error', text: err instanceof Error ? err.message : 'Failed to update profile.' });
    } finally {
      setLoading(false);
    }
  };

  if (!user) {
    return (
      <div className="container page">
        <p className="profile-login-required">Please log in to view your profile.</p>
      </div>
    );
  }

  // Must verify Aadhaar before creating/editing profile (name, email, phone, etc.)
  if (!user.aadhaarVerified) {
    return (
      <div className="container page profile-page">
        <h1 className="page-title">{t('profile.title')}</h1>
        <p className="page-subtitle profile-verify-subtitle">{t('profile.verifyAadhaarFirst')}</p>
        <form onSubmit={handleVerifyAadhaar} className="profile-form card profile-verify-card">
          {message && (
            <div className={`profile-message ${message.type}`} role="alert">
              {message.text}
            </div>
          )}
          <div className="form-row">
            <label>
              <span>{t('profile.aadhaarNumber')}</span>
              <input
                type="text"
                value={aadhaarNumber}
                onChange={(e) => setAadhaarNumber(e.target.value.replace(/\D/g, '').slice(0, 12))}
                placeholder="12 digits"
                maxLength={12}
                inputMode="numeric"
                required
              />
              <span className="profile-hint">{t('profile.aadhaarHintSecure')}</span>
            </label>
          </div>
          <div className="form-actions">
            <button type="submit" className="btn btn-primary" disabled={loading}>
              {loading ? t('profile.saving') : t('profile.verifyAadhaarSubmit')}
            </button>
          </div>
        </form>
      </div>
    );
  }

  return (
    <div className="container page profile-page">
      <h1 className="page-title">{t('profile.title')}</h1>
      <p className="page-subtitle">{t('profile.subtitle')}</p>

      <form onSubmit={handleSubmit} className="profile-form card">
        {message && (
          <div className={`profile-message ${message.type}`} role="alert">
            {message.text}
          </div>
        )}

        <div className="form-row">
          <label>
            <span>{t('profile.fullName')}</span>
            <input
              type="text"
              value={fullName}
              onChange={(e) => setFullName(e.target.value)}
              placeholder={t('profile.fullName')}
              required
            />
          </label>
        </div>
        <div className="form-row">
          <label>
            <span>{t('profile.email')}</span>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="email@example.com"
              required
            />
          </label>
        </div>
        <div className="form-row">
          <label>
            <span>{t('profile.phone')}</span>
            <input
              type="tel"
              value={phone}
              onChange={(e) => setPhone(e.target.value)}
              placeholder="9876543210"
              required
            />
          </label>
        </div>
        <div className="form-row">
          <label>
            <span>{t('profile.aadhaarNumber')}</span>
            {user.aadhaarVerified && (
              <p className="profile-aadhaar-masked">{t('profile.aadhaarVerified')}</p>
            )}
            <input
              type="text"
              value={aadhaarNumber}
              onChange={(e) => setAadhaarNumber(e.target.value.replace(/\D/g, '').slice(0, 12))}
              placeholder={t('profile.aadhaarOptionalPlaceholder')}
              maxLength={12}
              inputMode="numeric"
              pattern="[0-9]*"
              autoComplete="off"
            />
            <span className="profile-hint">{t('profile.aadhaarHintSecure')}</span>
          </label>
        </div>
        <div className="form-row">
          <label>
            <span>{t('profile.preferredLanguage')}</span>
            <select
              value={preferredLanguage}
              onChange={(e) => setPreferredLanguage(e.target.value)}
            >
              {LANGUAGES.map(({ value, label }) => (
                <option key={value} value={value}>{label}</option>
              ))}
            </select>
          </label>
        </div>

        <div className="profile-readonly">
          <p className="profile-readonly-label">{t('profile.role')}</p>
          <p className="profile-readonly-value">{user.roles.join(', ')}</p>
          {user.hospitalId != null && (
            <>
              <p className="profile-readonly-label">{t('profile.hospital')}</p>
              <p className="profile-readonly-value">ID: {user.hospitalId}</p>
            </>
          )}
          <p className="profile-readonly-note">{t('profile.cannotChange')}</p>
        </div>

        <div className="form-actions">
          <button type="submit" className="btn btn-primary" disabled={loading}>
            {loading ? t('profile.saving') : t('profile.save')}
          </button>
        </div>
      </form>
    </div>
  );
}
