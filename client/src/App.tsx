import { Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import Layout from './components/Layout';
import Home from './pages/Home';
import Hospitals from './pages/Hospitals';
import HospitalDetail from './pages/HospitalDetail';
import SymptomChecker from './pages/SymptomChecker';
import Emergency from './pages/Emergency';
import Login from './pages/Login';
import Register from './pages/Register';
import MyAppointments from './pages/MyAppointments';
import BookAppointment from './pages/BookAppointment';
import Profile from './pages/Profile';
import AdminDashboard from './pages/admin/AdminDashboard';
import AdminHospitals from './pages/admin/AdminHospitals';
import AdminDoctors from './pages/admin/AdminDoctors';
import AdminAppointments from './pages/admin/AdminAppointments';
import CreateHospitalAdmin from './pages/admin/CreateHospitalAdmin';
import AdminHospitalDetail from './pages/admin/AdminHospitalDetail';
import AdminDoctorDetail from './pages/admin/AdminDoctorDetail';

function Protected({ children, roles }: { children: React.ReactNode; roles?: string[] }) {
  const { user, loading } = useAuth();
  if (loading) return <div className="container" style={{ padding: '2rem', textAlign: 'center' }}>Loading...</div>;
  if (!user) return <Navigate to="/login" replace />;
  if (roles && roles.length && !roles.some((r) => user.roles.includes(r))) return <Navigate to="/" replace />;
  return <>{children}</>;
}

function AppRoutes() {
  return (
    <Routes>
      <Route path="/" element={<Layout />}>
        <Route index element={<Home />} />
        <Route path="hospitals" element={<Hospitals />} />
        <Route path="hospitals/:id" element={<HospitalDetail />} />
        <Route path="symptom-checker" element={<SymptomChecker />} />
        <Route path="emergency" element={<Emergency />} />
        <Route path="login" element={<Login />} />
        <Route path="register" element={<Register />} />
        <Route path="my-appointments" element={<Protected><MyAppointments /></Protected>} />
        <Route path="profile" element={<Protected><Profile /></Protected>} />
        <Route path="book/:doctorId" element={<Protected><BookAppointment /></Protected>} />
        <Route path="admin" element={<Protected roles={['SuperAdmin', 'HospitalAdmin']}><AdminDashboard /></Protected>} />
        <Route path="admin/hospitals" element={<Protected roles={['SuperAdmin', 'HospitalAdmin']}><AdminHospitals /></Protected>} />
        <Route path="admin/hospitals/:id" element={<Protected roles={['SuperAdmin', 'HospitalAdmin']}><AdminHospitalDetail /></Protected>} />
        <Route path="admin/doctors" element={<Protected roles={['SuperAdmin', 'HospitalAdmin']}><AdminDoctors /></Protected>} />
        <Route path="admin/doctors/:id" element={<Protected roles={['SuperAdmin', 'HospitalAdmin']}><AdminDoctorDetail /></Protected>} />
        <Route path="admin/appointments" element={<Protected roles={['SuperAdmin', 'HospitalAdmin']}><AdminAppointments /></Protected>} />
        <Route path="admin/create-hospital-admin" element={<Protected roles={['SuperAdmin']}><CreateHospitalAdmin /></Protected>} />
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}

export default function App() {
  return (
    <AuthProvider>
      <AppRoutes />
    </AuthProvider>
  );
}
