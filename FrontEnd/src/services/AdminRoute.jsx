import { useState, useEffect } from 'react';
import { Navigate } from 'react-router-dom';
import { getToken } from './AuthServices';

function AdminRoute({ children }) {
  const [isAdmin, setIsAdmin] = useState(null);
  const token = getToken();

  useEffect(() => {
    const checkAdmin = async () => {
      if (!token) {
        setIsAdmin(false);
        return;
      }

      try {
        const response = await fetch('https://dut-clinic-booking-system-y9d7.onrender.com/api/Admin/verifyAdmin', {
          headers: { 'Authorization': `Bearer ${token}` }
        });

        if (response.ok) {
          const data = await response.json();
          setIsAdmin(data.isAdmin === true);
        } else {
          setIsAdmin(false);
        }
      } catch (error) {
        console.error('Admin check failed:', error);
        setIsAdmin(false);
      }
    };

    checkAdmin();
  }, [token]);

  if (isAdmin === null) {
    return (
      <div style={{ 
        minHeight: '100vh', 
        display: 'flex', 
        alignItems: 'center', 
        justifyContent: 'center',
        color: 'white',
        fontSize: '1.2rem'
      }}>
        Checking permissions...
      </div>
    );
  }

  if (!token) {
    return <Navigate to="/login" replace />;
  }

  if (!isAdmin) {
    return <Navigate to="/" replace />;
  }

  return children;
}

export default AdminRoute;