import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { api, type SymptomResult } from '../api/client';
import './SymptomChecker.css';

export default function SymptomChecker() {
  const { t } = useTranslation();
  const [symptoms, setSymptoms] = useState('');
  const [age, setAge] = useState('');
  const [gender, setGender] = useState('');
  const [medicalHistory, setMedicalHistory] = useState('');
  const [result, setResult] = useState<SymptomResult | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleAnalyze = async () => {
    if (!symptoms.trim()) {
      setError('Please describe your symptoms.');
      return;
    }
    setError('');
    setLoading(true);
    setResult(null);
    try {
      const data = await api.symptoms.analyze({
        symptomsText: symptoms.trim(),
        age: age ? parseInt(age, 10) : undefined,
        gender: gender || undefined,
        medicalHistory: medicalHistory || undefined,
      });
      setResult(data);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Analysis failed.');
    } finally {
      setLoading(false);
    }
  };

  const quickSymptoms = ['Fever', 'Headache', 'Cough', 'Cold', 'Body Pain', 'Stomach Pain', 'Vomiting', 'Diarrhea', 'Fatigue', 'Dizziness'];

  return (
    <div className="container page symptom-checker-page">
      <h1 className="page-title">🩺 {t('symptoms.title')}</h1>
      <p className="page-subtitle">{t('symptoms.subtitle')}</p>

      <div className="symptom-layout">
        <div className="symptom-form card">
          <h3>{t('symptoms.describeTitle')}</h3>
          <p className="quick-label">Quick select common symptoms:</p>
          <div className="quick-symptoms">
            {quickSymptoms.map((s) => (
              <button
                key={s}
                type="button"
                className={`pill ${symptoms.toLowerCase().includes(s.toLowerCase()) ? 'pill-selected' : ''}`}
                onClick={() => setSymptoms((prev) => (prev ? `${prev}, ${s}` : s))}
              >
                {s}
              </button>
            ))}
          </div>
          <label>
            <span>Your Symptoms *</span>
            <textarea
              rows={4}
              placeholder={t('symptoms.symptomsPlaceholder')}
              value={symptoms}
              onChange={(e) => setSymptoms(e.target.value)}
            />
          </label>
          <label>
            <span>{t('symptoms.age')}</span>
            <input type="number" placeholder={t('symptoms.agePlaceholder')} value={age} onChange={(e) => setAge(e.target.value)} />
          </label>
          <label>
            <span>{t('symptoms.gender')}</span>
            <select value={gender} onChange={(e) => setGender(e.target.value)}>
              <option value="">{t('symptoms.genderSelect')}</option>
              <option value="Male">Male</option>
              <option value="Female">Female</option>
              <option value="Other">Other</option>
            </select>
          </label>
          <label>
            <span>{t('symptoms.medicalHistory')}</span>
            <input type="text" placeholder={t('symptoms.medicalHistoryPlaceholder')} value={medicalHistory} onChange={(e) => setMedicalHistory(e.target.value)} />
          </label>
          <button type="button" className="btn btn-primary btn-analyze" onClick={handleAnalyze} disabled={loading}>
            <span className="btn-analyze-icon">📈</span>
            {t('symptoms.analyze')}
          </button>
          <div className="disclaimer-left">
            <span className="disclaimer-icon">⚠️</span>
            <div>
              <strong>Important Disclaimer</strong>
              <p>{t('symptoms.disclaimer')}</p>
            </div>
          </div>
        </div>

        <div className="symptom-result card">
          {result ? (
            <div className="result-content">
              <div className={`urgency-box urgency-${(result.urgencyLevel || 'Routine').toLowerCase()}`}>
                <span className="urgency-icon">🕐</span>
                <strong>Urgency Level</strong>
                <span className="urgency-value">{result.urgencyLevel || 'Routine'}</span>
              </div>
              {result.analysis && (
                <div className="result-section">
                  <h4 className="result-section-title"><span className="section-icon">📈</span> Analysis</h4>
                  <p className="result-analysis">{result.analysis}</p>
                </div>
              )}
              <div className="result-section">
                <h4 className="result-section-title"><span className="section-icon">🩺</span> Recommended Specialty</h4>
                <span className="specialty-pill">{result.suggestedSpecialty}</span>
                {(() => {
                  const specForApi = result.suggestedSpecialty.replace(/General Physician \/ Internal Medicine/i, 'General Medicine').trim();
                  return (
                    <Link
                      to={specForApi ? `/hospitals?specialty=${encodeURIComponent(specForApi)}` : '/hospitals'}
                      className="btn btn-primary btn-find-doctors"
                    >
                      📍 Find {result.suggestedSpecialty} Doctors →
                    </Link>
                  );
                })()}
              </div>
              {result.suggestedActions && result.suggestedActions.length > 0 && (
                <div className="result-section suggested-actions">
                  <h4 className="result-section-title"><span className="section-icon">✅</span> Suggested Actions</h4>
                  <ol className="actions-list">
                    {result.suggestedActions.map((action, i) => (
                      <li key={i}>{action}</li>
                    ))}
                  </ol>
                </div>
              )}
            </div>
          ) : (
            <div className="result-placeholder">
              <span className="result-icon">🩺</span>
              <p>{t('symptoms.enterPrompt')}</p>
              <p className="muted">{t('symptoms.enterPromptDesc')}</p>
            </div>
          )}
          {error && <p className="error-msg">{error}</p>}
        </div>
      </div>
    </div>
  );
}
