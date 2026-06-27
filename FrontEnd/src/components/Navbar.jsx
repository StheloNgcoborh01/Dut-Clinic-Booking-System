import { Link, useNavigate } from 'react-router-dom';
import '../styles/Navbar.css';
import { Button } from '@mui/material';
import Panel from '../context/Panel.jsx';
import { useState, useEffect } from 'react';
import { isLoggedIn, logout } from '../services/AuthServices';

function Navbar({ togglePanel, isMobile }) {

  const loggedIn = isLoggedIn();

  const handleLogout = () => {
    logout(); 
  };


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
          <a href="/" draggable="false">
              Home
          </a>
        </li>

        <li>
          <a href="/contact" draggable="false">
            Contact Us
          </a>
        </li>

        <li>
          {
            loggedIn && (
          <a href="/bookings" draggable="false">
            My Bookings
          </a>
            )
          }

        
        </li>

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
              <a href='/login' draggable="false"> Log in </a>
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
