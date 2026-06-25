import React from 'react';
import '../styles/panel.css';

const Panel = ({ closePanel }) => {
  return (
    <div className="panel-overlay" onClick={closePanel}>
      <div className="PanelMobile" onClick={(e) => e.stopPropagation()}>
        <button className="panel-close" onClick={closePanel}>✕</button>
        <ul>
          <li><a href="/" onClick={closePanel}>Home</a></li>
          <li><a href="/contact" onClick={closePanel}>Contact</a></li>
          <li><a href="/bookings" onClick={closePanel}>My Bookings</a></li>
          <li><a href="/logout" onClick={closePanel}>Logout</a></li>
        </ul>
      </div>
    </div>
  );
};

export default Panel;