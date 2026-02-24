import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import './Admin.css';

function UserIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden>
      <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" />
      <circle cx="12" cy="7" r="4" />
    </svg>
  );
}
function StethoscopeIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden>
      <path d="M11 2v2" /><path d="M5 2v2" /><path d="M5 3H4a2 2 0 0 0-2 2v5a6 6 0 0 0 6 6v0a6 6 0 0 0 6-6V5a2 2 0 0 0-2-2h-1V2" />
      <path d="M8 15a6 6 0 0 0 6-6V5a2 2 0 0 0-2-2" />
      <circle cx="20" cy="10" r="2" /><path d="M20 12v2a4 4 0 0 1-4 4h-4" />
    </svg>
  );
}
function LocationIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden>
      <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z" /><circle cx="12" cy="10" r="3" />
    </svg>
  );
}
function CalendarIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden>
      <rect x="3" y="4" width="18" height="18" rx="2" ry="2" />
      <line x1="16" y1="2" x2="16" y2="6" /><line x1="8" y1="2" x2="8" y2="6" /><line x1="3" y1="10" x2="21" y2="10" />
    </svg>
  );
}
function PhoneIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden>
      <path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.84 12.84 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.7A2 2 0 0 1 22 16.92z" />
    </svg>
  );
}
function AlertCircleIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden>
      <circle cx="12" cy="12" r="10" />
      <line x1="12" y1="8" x2="12" y2="12" />
      <line x1="12" y1="16" x2="12.01" y2="16" />
    </svg>
  );
}

export default function AdminDashboard() {
  const { t } = useTranslation();
  const { user } = useAuth();

  const quickActions = [
    {
      to: '/symptom-checker',
      icon: StethoscopeIcon,
      iconClass: 'admin-qa-icon-teal',
      titleKey: 'dashboard.aiSymptomAnalysis',
      descKey: 'dashboard.aiSymptomAnalysisDesc',
    },
    {
      to: '/hospitals',
      icon: LocationIcon,
      iconClass: 'admin-qa-icon-blue',
      titleKey: 'dashboard.findHospitals',
      descKey: 'dashboard.findHospitalsDesc',
    },
    {
      to: '/my-appointments',
      icon: CalendarIcon,
      iconClass: 'admin-qa-icon-purple',
      titleKey: 'dashboard.myAppointments',
      descKey: 'dashboard.myAppointmentsDesc',
    },
    {
      to: '/emergency',
      icon: PhoneIcon,
      iconClass: 'admin-qa-icon-orange',
      titleKey: 'dashboard.emergency',
      descKey: 'dashboard.emergencyDesc',
    },
  ];

  const adminActions = [
    { to: '/admin/hospitals', icon: LocationIcon, titleKey: 'dashboard.adminHospitals', descKey: 'dashboard.adminHospitalsDesc' },
    { to: '/admin/doctors', icon: UserIcon, titleKey: 'dashboard.adminDoctors', descKey: 'dashboard.adminDoctorsDesc' },
    { to: '/admin/appointments', icon: CalendarIcon, titleKey: 'dashboard.adminAppointments', descKey: 'dashboard.adminAppointmentsDesc' },
  ];
  const isSuperAdmin = user?.roles.includes('SuperAdmin');

  return (
    <div className="container page admin-dashboard-page">
      <section className="admin-welcome-section">
        <div className="admin-welcome-text">
          <h1 className="admin-welcome-title">{t('dashboard.welcomeTitle')}</h1>
          <p className="admin-welcome-subtitle">{t('dashboard.welcomeSubtitle')}</p>
        </div>
        <div className="admin-welcome-aadhaar">
          <UserIcon className="admin-welcome-user-icon" aria-hidden />
          <span>{t('dashboard.aadhaarLabel')}</span>
        </div>
      </section>

      <section className="admin-quick-actions">
        <h2 className="admin-section-title">{t('dashboard.quickActions')}</h2>
        <div className="admin-qa-grid">
          {quickActions.map(({ to, icon: Icon, iconClass, titleKey, descKey }) => (
            <Link key={to} to={to} className="admin-qa-card">
              <span className={`admin-qa-icon-wrap ${iconClass}`} aria-hidden>
                <Icon className="admin-qa-icon" />
              </span>
              <h3 className="admin-qa-title">{t(titleKey)}</h3>
              <p className="admin-qa-desc">{t(descKey)}</p>
            </Link>
          ))}
        </div>
      </section>

      {(isSuperAdmin || user?.roles.includes('HospitalAdmin')) && (
        <section className="admin-quick-actions admin-admin-actions">
          <h2 className="admin-section-title">{t('dashboard.adminSection')}</h2>
          <div className="admin-qa-grid admin-admin-grid">
            {adminActions.map(({ to, icon: Icon, titleKey, descKey }) => (
              <Link key={to} to={to} className="admin-qa-card">
                <span className="admin-qa-icon-wrap admin-qa-icon-teal" aria-hidden>
                  <Icon className="admin-qa-icon" />
                </span>
                <h3 className="admin-qa-title">{t(titleKey)}</h3>
                <p className="admin-qa-desc">{t(descKey)}</p>
              </Link>
            ))}
            {isSuperAdmin && (
              <Link to="/admin/create-hospital-admin" className="admin-qa-card">
                <span className="admin-qa-icon-wrap admin-qa-icon-orange" aria-hidden>
                  <UserIcon className="admin-qa-icon" />
                </span>
                <h3 className="admin-qa-title">{t('dashboard.createHospitalAdmin')}</h3>
                <p className="admin-qa-desc">{t('dashboard.createHospitalAdminDesc')}</p>
              </Link>
            )}
          </div>
        </section>
      )}

      <section className="admin-dashboard-emergency-banner">
        <div className="admin-dashboard-emergency-inner">
          <div className="admin-dashboard-emergency-text">
            <span className="admin-dashboard-emergency-circle" aria-hidden>
              <AlertCircleIcon className="admin-dashboard-emergency-circle-icon" />
            </span>
            <div>
              <h2 className="admin-dashboard-emergency-title">{t('dashboard.emergencyBannerTitle')}</h2>
              <p className="admin-dashboard-emergency-subtitle">{t('dashboard.emergencyBannerSubtitle')}</p>
            </div>
          </div>
          <Link to="/emergency" className="admin-dashboard-emergency-btn">
            <PhoneIcon className="admin-dashboard-emergency-btn-icon" aria-hidden />
            {t('dashboard.emergencyButton')}
          </Link>
        </div>
      </section>
    </div>
  );
}
