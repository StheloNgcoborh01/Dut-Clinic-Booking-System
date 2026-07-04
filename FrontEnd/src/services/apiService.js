import { getToken, logout } from './AuthServices';

const API_URL = 'https://dut-clinic-booking-system-y9d7.onrender.com/api';

export const apiRequest = async (endpoint, options = {}) => {
    const token = getToken();
    
    const response = await fetch(`${API_URL}${endpoint}`, { //A fetch request with dynamic headers ..challenging  shiii
        ...options,
        headers: {
            'Content-Type': 'application/json',
            ...(token && { 'Authorization': `Bearer ${token}` }),
            ...options.headers,
        },
    });

    if (response.status === 401) {
        logout();
        window.location.href = '/login';
        throw new Error('Session expired. Please login again.');
    }

    return response;
};