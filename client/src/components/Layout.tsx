import { Outlet } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Link, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import './Layout.css';

function LogoutIcon({ className, ...props }: React.SVGProps<SVGSVGElement>) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" {...props}>
      <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4" />
      <polyline points="16 17 21 12 16 7" />
      <line x1="21" y1="12" x2="9" y2="12" />
    </svg>
  );
}

export default function Layout() {
  const { t, i18n } = useTranslation();
  const { user, logout } = useAuth();
  const location = useLocation();
  const isActive = (path: string) => location.pathname === path || (path !== '/' && location.pathname.startsWith(path));

  const setLang = (lng: string) => {
    i18n.changeLanguage(lng);
    localStorage.setItem('lang', lng);
  };

  return (
    <div className="layout">
      <header className="header">
        <div className="container header-inner">
          <Link to="/" className="logo">
            <span className="logo-icon">❤</span>
            <span>{t('appName')}</span>
          </Link>
          <nav className="nav">
            <Link to="/" className={isActive('/') && location.pathname === '/' ? 'active' : ''}>{t('nav.home')}</Link>
            <Link to="/hospitals" className={isActive('/hospitals') ? 'active' : ''}>{t('nav.hospitals')}</Link>
            <Link to="/symptom-checker" className={isActive('/symptom-checker') ? 'active' : ''}>{t('nav.symptomChecker')}</Link>
            <Link to="/emergency" className={isActive('/emergency') ? 'active' : ''}>{t('nav.emergency')}</Link>
            {(user?.roles.includes('SuperAdmin') || user?.roles.includes('HospitalAdmin')) && (
              <Link to="/admin" className={isActive('/admin') ? 'active' : ''}>{t('nav.dashboard')}</Link>
            )}
            {user && (
              <Link to="/my-appointments" className={isActive('/my-appointments') ? 'active' : ''}>{t('nav.myAppointments')}</Link>
            )}
          </nav>
          <div className="header-actions">
            <div className="lang-select" aria-label={t('nav.language')}>
              <span className="lang-icon" aria-hidden>🌐</span>
              <label htmlFor="lang-select" className="lang-label">{t('nav.language')}</label>
              <select id="lang-select" value={i18n.language} onChange={(e) => setLang(e.target.value)}>
                <option value="en">English</option>
                <option value="hi">हिंदी</option>
                <option value="te">తెలుగు</option>
              </select>
            </div>
            {user ? (
              <div className="user-menu">
                <Link to="/profile" className="header-profile-link">{t('nav.profile')}</Link>
                {(user.roles.includes('SuperAdmin') || user.roles.includes('HospitalAdmin')) && (
                  <span className="header-admin-label">{t('nav.admin')}</span>
                )}
                <button type="button" className="btn btn-outline header-logout-btn" onClick={logout} title={t('auth.logout')}>
                  <LogoutIcon className="header-logout-icon" aria-hidden />
                  {t('auth.logout')}
                </button>
              </div>
            ) : (
              <>
                <Link to="/login" className="link-login">{t('auth.login')}</Link>
                <Link to="/register" className="btn btn-primary">{t('auth.register')}</Link>
              </>
            )}
          </div>
        </div>
      </header>
      <main className="main">
        <Outlet />
      </main>
      <footer className="footer">
        <div className="container footer-inner">
          <div className="footer-brand">
            <span className="logo-icon">❤</span>
            <span>{t('appName')}</span>
            <p>{t('footer.tagline')}</p>
          </div>
          <div className="footer-links">
            <h4>{t('footer.quickLinks')}</h4>
            <Link to="/hospitals">{t('nav.hospitals')}</Link>
            <Link to="/symptom-checker">{t('nav.symptomChecker')}</Link>
            <Link to="/emergency">{t('nav.emergency')}</Link>
          </div>
          <div className="footer-emergency">
            <h4>{t('footer.emergencyNumbers')}</h4>
            <p>108, 100, 101, 181, 1098</p>
          </div>
        </div>
        <div className="footer-copy">{t('footer.copyright')}</div>
      </footer>
    </div>
  );
}
