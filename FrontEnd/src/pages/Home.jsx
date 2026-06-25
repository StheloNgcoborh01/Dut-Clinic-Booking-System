import Navbar from '../components/Navbar';
import '../styles/Home.css';
import { useState, useEffect } from 'react';
import Panel from '../context/Panel.jsx'
import { Link } from 'react-router-dom';


function Home() {

  const [showPanel, setShowPanel] = useState(false); // les state is boolean if a use pres the show history it true ..then history panel is shown
  const [isMobile, setIsMobile] = useState(window.innerWidth <= 768); // le state si check screen size then return true or false bas eon it..

   // Listen for window resize to toggle mobile state
  useEffect(() => {
    const handleResize = () => {
      setIsMobile(window.innerWidth <= 768); // if the arguemnt is true..then the ismobile is true..
      if (window.innerWidth > 768) setShowPanel(false); // hide mobile panel on desktop iif it false the the value is false
    };
    window.addEventListener("resize", handleResize); // this listens to the resize then pas the value of true or false
    return () => window.removeEventListener("resize", handleResize);
  }, []);


  return (
    <div className="home-page">
      <Navbar
       togglePanel = {() => setShowPanel((prev) => !prev)} 
      isMobile={isMobile} 
       />

             {showPanel && isMobile && <Panel closePanel={() => setShowPanel(false)} />}
      
      <div className="hero-section">
        <div className="hero-content">
          <h1>Your health is our priority</h1>
          <p>Book appointments, manage your health, and stay connected with Ntuzuma Clinic.</p>
   
     <Link to="/AddBooking" className="hero-btn">📅 Book Appointment</Link>        </div>
      </div>
    </div>
  );
}

export default Home;