const API_BASE = '/api';

let accessToken: string | null = null;

export function setAccessToken(token: string) {
  accessToken = token;
}
export function clearAccessToken() {
  accessToken = null;
}
export function getAccessToken() {
  return accessToken;
}

async function request<T>(
  path: string,
  options: RequestInit = {}
): Promise<T> {
  const url = path.startsWith('http') ? path : `${API_BASE}${path}`;
  const headers: HeadersInit = {
    'Content-Type': 'application/json',
    ...(options.headers as Record<string, string>),
  };
  if (accessToken) (headers as Record<string, string>)['Authorization'] = `Bearer ${accessToken}`;
  const res = await fetch(url, { ...options, headers });
  if (!res.ok) {
    const err = await res.json().catch(() => ({ message: res.statusText }));
    throw new Error((err as { message?: string }).message || res.statusText);
  }
  if (res.status === 204) return undefined as T;
  return res.json();
}

export const api = {
  auth: {
    register: (body: { email: string; phone: string; password: string; fullName: string; aadhaarNumber?: string; preferredLanguage?: string }) =>
      request<{ accessToken: string; refreshToken: string; expiresAtUtc: string; user: UserInfo }>(`/auth/register`, { method: 'POST', body: JSON.stringify(body) }),
    login: (body: { emailOrPhone: string; password: string }) =>
      request<{ accessToken: string; refreshToken: string; expiresAtUtc: string; user: UserInfo }>(`/auth/login`, { method: 'POST', body: JSON.stringify(body) }),
    refresh: (refreshToken: string) =>
      request<{ accessToken: string; refreshToken: string; user: UserInfo }>(`/auth/refresh`, { method: 'POST', body: JSON.stringify({ refreshToken }) }),
    me: () => request<UserInfo>(`/auth/me`),
  },
  hospitals: {
    list: (params?: { search?: string; specialty?: string; type?: string }) => {
      const clean = Object.fromEntries(
        Object.entries(params ?? {}).filter(([, v]) => v != null && v !== '')
      ) as Record<string, string>;
      const q = new URLSearchParams(clean).toString();
      return request<HospitalListItem[]>(`/hospitals${q ? `?${q}` : ''}`);
    },
    get: (id: number) => request<HospitalDetail>(`/hospitals/${id}`),
    nearest: (params?: { lat?: number; lon?: number; count?: number; emergencyOnly?: boolean }) => {
      const clean = Object.fromEntries(
        Object.entries(params ?? {}).filter(([, v]) => v != null && v !== '')
      ) as Record<string, string>;
      const q = new URLSearchParams(clean).toString();
      return request<HospitalListItem[]>(`/hospitals/nearest${q ? `?${q}` : ''}`);
    },
  },
  doctors: {
    list: (params?: { hospitalId?: number; specialty?: string }) => {
      const clean = Object.fromEntries(
        Object.entries(params ?? {}).filter(([, v]) => v != null && v !== '')
      ) as Record<string, string>;
      const q = new URLSearchParams(clean).toString();
      return request<DoctorListItem[]>(`/doctors${q ? `?${q}` : ''}`);
    },
    get: (id: number) => request<DoctorDetail>(`/doctors/${id}`),
  },
  appointments: {
    slots: (params: { doctorId: number; from: string; to: string; consultationType?: string }) => {
      const q = new URLSearchParams(params as Record<string, string>).toString();
      return request<SlotDto[]>(`/appointments/slots?${q}`);
    },
    book: (body: { slotId: number; chiefComplaint?: string }) =>
      request<AppointmentDto>(`/appointments/book`, { method: 'POST', body: JSON.stringify(body) }),
    my: () => request<AppointmentDto[]>(`/appointments/my`),
  },
  symptoms: {
    analyze: (body: { symptomsText: string; age?: number; gender?: string; medicalHistory?: string }) =>
      request<SymptomResult>(`/symptoms/analyze`, { method: 'POST', body: JSON.stringify(body) }),
  },
  emergency: {
    contacts: () => request<EmergencyContact[]>(`/emergency/contacts`),
    nearestHospitals: (params?: { lat?: number; lon?: number; count?: number }) => {
      const clean = Object.fromEntries(
        Object.entries(params ?? {}).filter(([, v]) => v != null && v !== '')
      ) as Record<string, string>;
      const q = new URLSearchParams(clean).toString();
      return request<HospitalListItem[]>(`/emergency/nearest-hospitals${q ? `?${q}` : ''}`);
    },
  },
  admin: {
    hospitals: {
      list: () => request<HospitalListItem[]>(`/admin/AdminHospitals`),
      get: (id: number) => request<HospitalDetail>(`/admin/AdminHospitals/${id}`),
      create: (body: CreateHospitalBody) => request<HospitalListItem>(`/admin/AdminHospitals`, { method: 'POST', body: JSON.stringify(body) }),
      update: (id: number, body: UpdateHospitalBody) => request<void>(`/admin/AdminHospitals/${id}`, { method: 'PUT', body: JSON.stringify(body) }),
    },
    doctors: {
      list: (hospitalId?: number) => request<DoctorListItem[]>(`/admin/AdminDoctors${hospitalId != null ? `?hospitalId=${hospitalId}` : ''}`),
      create: (body: CreateDoctorBody) => request<DoctorListItem>(`/admin/AdminDoctors`, { method: 'POST', body: JSON.stringify(body) }),
      update: (id: number, body: UpdateDoctorBody) => request<void>(`/admin/AdminDoctors/${id}`, { method: 'PUT', body: JSON.stringify(body) }),
    },
    appointments: {
      list: (hospitalId: number, date?: string) => request<AppointmentDto[]>(`/admin/AdminAppointments?hospitalId=${hospitalId}${date ? `&date=${date}` : ''}`),
    },
    users: {
      createHospitalAdmin: (body: CreateHospitalAdminBody) => request<{ id: number }>(`/admin/AdminUsers/hospital-admin`, { method: 'POST', body: JSON.stringify(body) }),
    },
  },
};

export interface CreateHospitalBody {
  name: string;
  address: string;
  area?: string;
  phone: string;
  email?: string;
  type: string;
  bedCount: number;
  latitude?: number;
  longitude?: number;
  isEmergency: boolean;
}
export interface UpdateHospitalBody {
  name?: string;
  address?: string;
  area?: string;
  phone?: string;
  email?: string;
  type?: string;
  bedCount?: number;
  latitude?: number;
  longitude?: number;
  isEmergency?: boolean;
}
export interface CreateDoctorBody {
  hospitalId?: number;
  fullName: string;
  qualification?: string;
  registrationNumber?: string;
  phone?: string;
  email?: string;
  supportsInPerson: boolean;
  supportsVideo: boolean;
  specialtyIds?: number[];
}
export interface UpdateDoctorBody {
  fullName?: string;
  qualification?: string;
  registrationNumber?: string;
  phone?: string;
  email?: string;
  supportsInPerson?: boolean;
  supportsVideo?: boolean;
}
export interface CreateHospitalAdminBody {
  email: string;
  phone: string;
  password: string;
  fullName: string;
  hospitalId: number;
  preferredLanguage?: string;
}

export interface UserInfo {
  id: number;
  email: string;
  phone: string;
  fullName: string;
  preferredLanguage: string;
  aadhaarVerified: boolean;
  roles: string[];
  hospitalId?: number;
}

export interface HospitalListItem {
  id: number;
  name: string;
  address: string;
  area?: string;
  phone: string;
  type: string;
  bedCount: number;
  latitude?: number;
  longitude?: number;
  isEmergency: boolean;
  specialties: string[];
  distanceKm?: number;
}

export interface HospitalDetail extends HospitalListItem {
  email?: string;
  doctors: { id: number; fullName: string; qualification?: string; specialties: string[]; supportsInPerson: boolean; supportsVideo: boolean; phone?: string }[];
}

export interface DoctorListItem {
  id: number;
  hospitalId: number;
  hospitalName: string;
  fullName: string;
  qualification?: string;
  specialties: string[];
  supportsInPerson: boolean;
  supportsVideo: boolean;
  phone?: string;
}

export interface DoctorDetail extends DoctorListItem {
  email?: string;
  registrationNumber?: string;
}

export interface SlotDto {
  id: number;
  doctorId: number;
  doctorName: string;
  hospitalId: number;
  hospitalName: string;
  date: string;
  startTime: string;
  endTime: string;
  consultationType: string;
}

export interface AppointmentDto {
  id: number;
  slotId: number;
  date: string;
  startTime: string;
  consultationType: string;
  chiefComplaint?: string;
  status: string;
  doctorName: string;
  hospitalName: string;
  patientName: string;
}

export interface SymptomResult {
  urgencyLevel: string;
  analysis: string;
  suggestedSpecialty: string;
  possibleConditions: string[];
  suggestedActions: string[];
  disclaimer: string;
  recommendedHospitalIds: number[];
}

export interface EmergencyContact {
  name: string;
  number: string;
  description: string;
}
