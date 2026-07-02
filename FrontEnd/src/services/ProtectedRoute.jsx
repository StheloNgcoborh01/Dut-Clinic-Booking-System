import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { getToken, isLoggedIn } from '../services/AuthServices';
import { Navigate } from 'react-router-dom';


function ProtectedRoute({ children }) {
const loggedIn = isLoggedIn();

if (!loggedIn) {

   return <Navigate to ="/login" replace />;
   
}
  return children;

}

export default ProtectedRoute;