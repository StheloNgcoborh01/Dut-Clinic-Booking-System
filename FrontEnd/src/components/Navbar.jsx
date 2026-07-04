import { Link, useNavigate } from 'react-router-dom';
import '../styles/Navbar.css';
import Panel from '../context/Panel.jsx';
import { useState, useEffect } from 'react';
import { isLoggedIn, logout } from '../services/AuthServices';
import { checkAdminStatus } from '../services/AuthServices'; 


function Navbar({ togglePanel, isMobile }) {


  const loggedIn = isLoggedIn();
  const token = localStorage.getItem('token');

  const [isAdmin, setIsAdmin] = useState(false);
  const [adminChecked, setAdminChecked] = useState(false);

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
        const response = await fetch('https://dut-clinic-booking-system-y9d7.onrender.com/api/Admin/verifyAdmin', {
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
    <div className="Navbar">

<img
  src="/hospital-logo-design-vector-medical-cross_53876-136743.avif"
  alt="Ntuzuma Clinic logo"
  draggable="false"
  style={{
    height: "90px",
    width: "auto",
    objectFit: "contain",
    paddingLeft : "30px"
  }}
/>

       {!isMobile && (
        <>
        
      <ul className="navSelection" draggable="false">
        <li>
          <Link to="/" >
              Home
          </Link>
        </li>

        <li>
          <Link to="/contact" >
            Contact Us
          </Link>
        </li>

        <li>
          {
            loggedIn && (
          <Link to="/bookings" >
            My Bookings
          </Link>
            )
          }

        
        </li>

     
          {adminChecked && isAdmin && (
            <li><Link to="/admin/dashboard">Admin</Link></li>
          )}

        <li>
          {
           loggedIn && (
            <a onClick = {handleLogout} draggable="false">
           Log Out
          </a>
           )
          }

          {
            !loggedIn && (
              <Link to='/login' > Log in </Link>
            )
          }

        </li>
      </ul>
        </>

        )}

        {isMobile && (

          <button
           
            onClick={togglePanel}
            id="history-button"
          >
          <svg
    width="28"
    height="28"
    viewBox="0 0 24 24"
    fill="none"
    stroke="Black"
    strokeWidth="2"
    strokeLinecap="round"
    strokeLinejoin="round"
  >
    <line x1="3" y1="6" x2="21" y2="6" />
    <line x1="3" y1="12" x2="21" y2="12" />
    <line x1="3" y1="18" x2="21" y2="18" />
  </svg>
          </button>
        )}


    </div>
  );
}

export default Navbar;
