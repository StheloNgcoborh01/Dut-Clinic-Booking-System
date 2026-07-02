import{ React, useEffect ,  useState} from 'react';
import '../styles/panel.css';
import { isLoggedIn, logout } from '../services/AuthServices';
import { Link } from 'react-router-dom';

const Panel = ({ closePanel }) => {

  
    const [isAdmin, setIsAdmin] = useState(false);
    const [adminChecked, setAdminChecked] = useState(false);
    const token = localStorage.getItem('token');


    const loggedIn = isLoggedIn();

  const handleLogout = () => {
    logout(); 
  };

      useEffect(() => {
      const checkAdmin = async () => {
        if (!token) {
          setAdminChecked(true);
          return;
        }
  
        try {
          const response = await fetch('http://localhost:3000/api/Admin/verifyAdmin', {
            headers: { 'Authorization': `Bearer ${token}` }
          });
  
          if (response.ok) {
            const data = await response.json();
            setIsAdmin(data.isAdmin === true);
          }
        } catch (error) {
          console.error('Admin check failed:', error);
        } finally {
          setAdminChecked(true);
        }
      };
  
      checkAdmin();
    }, [token]);


  return (
    <div className="panel-overlay" onClick={closePanel}>
      <div className="PanelMobile" onClick={(e) => e.stopPropagation()}>
        <button className="panel-close" onClick={closePanel}>✕</button>
        <ul>
          <li><a href="/" onClick={closePanel}>Home</a></li>
          <li><a href="/contact" onClick={closePanel}>Contact</a></li>
     {
            loggedIn && (
            <li><a href="/bookings" onClick={closePanel}>My Bookings</a></li>
            )
          }

          {
            !loggedIn && (
          <li><a href="/login" onClick={closePanel}>Log In</a></li>

            )
          }


          {adminChecked && isAdmin && (
            <li><Link to="/admin/dashboard">Admin</Link></li>
          )}

          {
            loggedIn && (
            <li><a onClick = {handleLogout}> Log Out</a></li>
            )
          }
        </ul>
      </div>
    </div>
  );
};

export default Panel;