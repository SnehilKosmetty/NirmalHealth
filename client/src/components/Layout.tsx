import { Outlet } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Link, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import './Layout.css';

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
          </nav>
          <div className="header-actions">
            <div className="lang-select">
              <span className="lang-icon">🌐</span>
              <select value={i18n.language} onChange={(e) => setLang(e.target.value)}>
                <option value="en">English</option>
                <option value="hi">हिंदी</option>
                <option value="te">తెలుగు</option>
              </select>
            </div>
            {user ? (
              <div className="user-menu">
                {user.roles.includes('SuperAdmin') || user.roles.includes('HospitalAdmin') ? (
                  <Link to="/admin" className="btn btn-outline" style={{ marginRight: 8 }}>Admin</Link>
                ) : null}
                <Link to="/my-appointments" className="btn btn-outline" style={{ marginRight: 8 }}>My Appointments</Link>
                <button type="button" className="btn btn-outline" onClick={logout}>{t('auth.logout')}</button>
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
