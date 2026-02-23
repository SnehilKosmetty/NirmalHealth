import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import './Home.css';

export default function Home() {
  const { t } = useTranslation();

  return (
    <>
      <section className="hero">
        <div className="container hero-inner">
          <div className="hero-content">
            <span className="hero-badge">🛡 {t('home.badge')}</span>
            <h1 className="hero-title">{t('home.title')}</h1>
            <p className="hero-subtitle">{t('home.subtitle')}</p>
            <div className="hero-actions">
              <Link to="/register" className="btn btn-primary">
                {t('home.getStarted')} →
              </Link>
              <Link to="/hospitals" className="btn btn-outline">
                {t('home.findHospitals')}
              </Link>
            </div>
            <div className="hero-emergency-card">
              <span className="hero-emergency-icon">📞</span>
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
            <span className="stat-icon">❤</span>
            <span className="stat-value">{t('home.stats.hospitals')}</span>
          </div>
          <div className="stat-card">
            <span className="stat-icon">👥</span>
            <span className="stat-value">{t('home.stats.doctors')}</span>
          </div>
          <div className="stat-card">
            <span className="stat-icon">🕐</span>
            <span className="stat-value">{t('home.stats.emergency')}</span>
          </div>
          <div className="stat-card">
            <span className="stat-icon">🛡</span>
            <span className="stat-value">{t('home.stats.secure')}</span>
          </div>
        </div>
      </section>

      <section className="how-it-works">
        <div className="container">
          <h2 className="section-title">{t('howItWorks.title')}</h2>
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
