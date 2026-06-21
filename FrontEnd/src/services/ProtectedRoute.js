import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { getToken } from '../services/AuthServices';

function ProtectedRoute({ children }) {
  const navigate = useNavigate();
  const token = getToken();

  useEffect(() => {
    if (!token) {
      navigate('/login');
    }
  }, [token, navigate]);

  return token ? children : null;
}

export default ProtectedRoute;