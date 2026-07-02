// src/services/AuthServices.js

export const getToken = () => {
  return localStorage.getItem('token');
};

export const getTokenExpiration = (token) => {
  try {
    const payload = token.split('.')[1];
    const decoded = atob(payload);
    const parsed = JSON.parse(decoded);
    return parsed.exp;
  } catch (error) {
    console.error('Invalid token:', error);
    return null;
  }
};

export const isTokenExpired = (token) => {
  if (!token) return true;
  
  const exp = getTokenExpiration(token);
  if (!exp) return true;
  
  const now = Math.floor(Date.now() / 1000);
  return exp < now;
};

export const isLoggedIn = () => {
  const token = getToken();
  return token && !isTokenExpired(token);
};

export const logout = () => {
  localStorage.removeItem('token');
  window.location.href = '/login';
};


export const checkAdminStatus = async (token) => {
  try {
    const response = await fetch('http://localhost:3000/api/Admin/verifyAdmin', {
      headers: { 'Authorization': `Bearer ${token}` }
    });

    if (response.ok) {
      const data = await response.json();
      return data.isAdmin === true;
    }
    return false;
  } catch (error) {
    console.error('Admin check failed:', error);
    return false;
  }
};



