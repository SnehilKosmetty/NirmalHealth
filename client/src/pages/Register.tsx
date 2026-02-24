import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import './Auth.css';

export default function Register() {
  const { t } = useTranslation();
  const { register } = useAuth();
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [phone, setPhone] = useState('');
  const [password, setPassword] = useState('');
  const [fullName, setFullName] = useState('');
  const [aadhaarNumber, setAadhaarNumber] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const aadhaarDigits = aadhaarNumber.replace(/\D/g, '');
  const isAadhaarValid = aadhaarDigits.length === 12;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    if (!isAadhaarValid) {
      setError(t('register.aadhaarRequired'));
      return;
    }
    setLoading(true);
    try {
      await register({
        email: email.trim(),
        phone: phone.trim(),
        password,
        fullName: fullName.trim(),
        aadhaarNumber: aadhaarDigits,
      });
      navigate('/');
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Registration failed.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-page">
      <div className="auth-card card">
        <h1>{t('auth.register')}</h1>
        <form onSubmit={handleSubmit}>
          <label>
            <span>Full Name</span>
            <input type="text" value={fullName} onChange={(e) => setFullName(e.target.value)} required />
          </label>
          <label>
            <span>Email</span>
            <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
          </label>
          <label>
            <span>Phone</span>
            <input type="tel" value={phone} onChange={(e) => setPhone(e.target.value)} placeholder="10-digit mobile" required />
          </label>
          <label>
            <span>Password</span>
            <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required minLength={6} />
          </label>
          <label>
            <span>{t('register.aadhaarLabel')}</span>
            <input
              type="text"
              value={aadhaarNumber}
              onChange={(e) => setAadhaarNumber(e.target.value.replace(/\D/g, '').slice(0, 12))}
              placeholder={t('register.aadhaarPlaceholder')}
              maxLength={12}
              inputMode="numeric"
              required
            />
            <span className="auth-hint">{t('register.aadhaarHint')}</span>
          </label>
          {error && <p className="auth-error">{error}</p>}
          <button type="submit" className="btn btn-primary btn-block" disabled={loading}>
            {loading ? 'Creating account...' : t('auth.register')}
          </button>
        </form>
        <p className="auth-footer">
          Already have an account? <Link to="/login">{t('auth.login')}</Link>
        </p>
      </div>
    </div>
  );
}
