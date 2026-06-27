import React from 'react';
import '../styles/panel.css';
import { isLoggedIn, logout } from '../services/AuthServices';

const Panel = ({ closePanel }) => {
    const loggedIn = isLoggedIn();

  const handleLogout = () => {
    logout(); 
  };


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