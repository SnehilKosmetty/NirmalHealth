import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { api, type StatsDto } from '../api/client';
import './Home.css';

export default function Home() {
  const { t } = useTranslation();
  const { user } = useAuth();
  const [stats, setStats] = useState<StatsDto | null>(null);

  useEffect(() => {
    api.stats.get().then(setStats).catch(() => setStats(null));
  }, []);

  return (
    <>
      <section className="hero">
        <div className="container hero-inner">
          <div className="hero-content">
            <span className="hero-badge">
              <ShieldIcon className="hero-badge-icon" aria-hidden />
              {t('home.badge')}
            </span>
            <h1 className="hero-title">{t('home.title')}</h1>
            <p className="hero-subtitle">{t('home.subtitle')}</p>
            <div className="hero-actions">
              {user ? (
                <>
                  <Link to="/hospitals" className="btn btn-primary">
                    {t('home.findHospitals')} →
                  </Link>
                  <Link to="/my-appointments" className="btn btn-outline">
                    {t('home.myAppointments')}
                  </Link>
                </>
              ) : (
                <>
                  <Link to="/register" className="btn btn-primary">
                    {t('home.getStarted')} →
                  </Link>
                  <Link to="/hospitals" className="btn btn-outline">
                    {t('home.findHospitals')}
                  </Link>
                </>
              )}
            </div>
            <div className="hero-emergency-card">
              <PhoneIcon className="hero-emergency-icon" aria-hidden />
              <span>{t('home.emergency108')}</span>
            </div>
          </div>
          <div className="hero-image">
            <div className="hero-image-placeholder" />
          </div>
        </div>
      </section>

      <section className="stats">
        <div className="container stats-grid">
          <div className="stat-card">
            <span className="stat-icon-wrap" aria-hidden>
              <HeartIcon className="stat-icon" />
            </span>
            <span className="stat-value">
              {stats ? `${stats.hospitalsCount}+` : '—'}
            </span>
            <span className="stat-label">{t('home.stats.hospitalsLabel')}</span>
          </div>
          <div className="stat-card">
            <span className="stat-icon-wrap" aria-hidden>
              <DoctorsIcon className="stat-icon" />
            </span>
            <span className="stat-value">
              {stats ? `${stats.doctorsCount}+` : '—'}
            </span>
            <span className="stat-label">{t('home.stats.doctorsLabel')}</span>
          </div>
          <div className="stat-card">
            <span className="stat-icon-wrap" aria-hidden>
              <ClockIcon className="stat-icon" />
            </span>
            <span className="stat-value">24/7</span>
            <span className="stat-label">{t('home.stats.emergencyLabel')}</span>
          </div>
          <div className="stat-card">
            <span className="stat-icon-wrap" aria-hidden>
              <ShieldIcon className="stat-icon" />
            </span>
            <span className="stat-value">100%</span>
            <span className="stat-label">{t('home.stats.secureLabel')}</span>
          </div>
        </div>
      </section>

      <section className="features" aria-labelledby="features-heading">
        <div className="container">
          <h2 id="features-heading" className="features-title">{t('home.featuresTitle')}</h2>
          <p className="features-subtitle">{t('home.featuresSubtitle')}</p>
          <div className="features-grid">
            <div className="feature-card">
              <span className="feature-icon-wrap feature-icon-teal" aria-hidden>
                <StethoscopeIcon className="feature-icon" />
              </span>
              <h3 className="feature-card-title">{t('home.feature1Title')}</h3>
              <p className="feature-card-desc">{t('home.feature1Desc')}</p>
            </div>
            <div className="feature-card">
              <span className="feature-icon-wrap feature-icon-blue" aria-hidden>
                <LocationIcon className="feature-icon" />
              </span>
              <h3 className="feature-card-title">{t('home.feature2Title')}</h3>
              <p className="feature-card-desc">{t('home.feature2Desc')}</p>
            </div>
            <div className="feature-card">
              <span className="feature-icon-wrap feature-icon-purple" aria-hidden>
                <CalendarIcon className="feature-icon" />
              </span>
              <h3 className="feature-card-title">{t('home.feature3Title')}</h3>
              <p className="feature-card-desc">{t('home.feature3Desc')}</p>
            </div>
            <div className="feature-card">
              <span className="feature-icon-wrap feature-icon-orange" aria-hidden>
                <PhoneIcon className="feature-icon" />
              </span>
              <h3 className="feature-card-title">{t('home.feature4Title')}</h3>
              <p className="feature-card-desc">{t('home.feature4Desc')}</p>
            </div>
          </div>
        </div>
      </section>

      <section className="emergency-banner">
        <div className="container emergency-banner-inner">
          <div className="emergency-banner-text">
            <h2 className="emergency-banner-title">{t('home.emergencyBannerTitle')}</h2>
            <p className="emergency-banner-subtitle">{t('home.emergencyBannerSubtitle')}</p>
          </div>
          <Link to="/emergency" className="emergency-banner-btn">
            <PhoneIcon className="emergency-banner-btn-icon" aria-hidden />
            {t('home.emergencyHelp')}
          </Link>
        </div>
      </section>

      <section className="how-it-works">
        <div className="container">
          <h2 className="section-title">{t('howItWorks.title')}</h2>
          <p className="section-subtitle">{t('howItWorks.subtitle')}</p>
          <div className="steps">
            <div className="step">
              <div className="step-num">1</div>
              <h3>{t('howItWorks.step1')}</h3>
              <p>{t('howItWorks.step1Desc')}</p>
            </div>
            <div className="step-connector" />
            <div className="step">
              <div className="step-num">2</div>
              <h3>{t('howItWorks.step2')}</h3>
              <p>{t('howItWorks.step2Desc')}</p>
            </div>
            <div className="step-connector" />
            <div className="step">
              <div className="step-num">3</div>
              <h3>{t('howItWorks.step3')}</h3>
              <p>{t('howItWorks.step3Desc')}</p>
            </div>
          </div>
        </div>
      </section>
    </>
  );
}

function ShieldIcon({ className, ...props }: React.SVGProps<SVGSVGElement>) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" {...props}>
      <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z" />
    </svg>
  );
}
function PhoneIcon({ className, ...props }: React.SVGProps<SVGSVGElement>) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" {...props}>
      <path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.84 12.84 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.7A2 2 0 0 1 22 16.92z" />
    </svg>
  );
}
function HeartIcon({ className, ...props }: React.SVGProps<SVGSVGElement>) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" {...props}>
      <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z" />
    </svg>
  );
}
function DoctorsIcon({ className, ...props }: React.SVGProps<SVGSVGElement>) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" {...props}>
      <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" />
      <circle cx="9" cy="7" r="4" />
      <path d="M23 21v-2a4 4 0 0 0-3-3.87" />
      <path d="M16 3.13a4 4 0 0 1 0 7.75" />
    </svg>
  );
}
function ClockIcon({ className, ...props }: React.SVGProps<SVGSVGElement>) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" {...props}>
      <circle cx="12" cy="12" r="10" />
      <polyline points="12 6 12 12 16 14" />
    </svg>
  );
}
function StethoscopeIcon({ className, ...props }: React.SVGProps<SVGSVGElement>) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" {...props}>
      <path d="M11 2v2" />
      <path d="M5 2v2" />
      <path d="M5 3H4a2 2 0 0 0-2 2v5a6 6 0 0 0 6 6v0a6 6 0 0 0 6-6V5a2 2 0 0 0-2-2h-1V2" />
      <path d="M8 15a6 6 0 0 0 6-6V5a2 2 0 0 0-2-2" />
      <circle cx="20" cy="10" r="2" />
      <path d="M20 12v2a4 4 0 0 1-4 4h-4" />
    </svg>
  );
}
function LocationIcon({ className, ...props }: React.SVGProps<SVGSVGElement>) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" {...props}>
      <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z" />
      <circle cx="12" cy="10" r="3" />
    </svg>
  );
}
function CalendarIcon({ className, ...props }: React.SVGProps<SVGSVGElement>) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" {...props}>
      <rect x="3" y="4" width="18" height="18" rx="2" ry="2" />
      <line x1="16" y1="2" x2="16" y2="6" />
      <line x1="8" y1="2" x2="8" y2="6" />
      <line x1="3" y1="10" x2="21" y2="10" />
    </svg>
  );
}
