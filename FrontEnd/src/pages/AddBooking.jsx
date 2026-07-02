import { useState, useEffect } from 'react';
import Navbar from '../components/Navbar';
import Panel from '../context/Panel';
import '../styles/AddBooking.css';
import {  useNavigate } from 'react-router-dom';

function AddBooking() {
  const navigate = useNavigate();
  const [showPanel, setShowPanel] = useState(false);
  const [isMobile, setIsMobile] = useState(window.innerWidth <= 768);


  const [availability, setAvailability] = useState({});

  // Stores start and end dates returned from backend
  const [startDate, setStartDate] = useState("");
  const [endDate, setEndDate] = useState("");

 

  // User selections
  const [selectedDate, setSelectedDate] = useState("");
  const [selectedTime, setSelectedTime] = useState("");

  // Stores times for the currently selected date
  const [availableTimes, setAvailableTimes] = useState([]);

  useEffect(() => {
    const handleResize = () => {
      setIsMobile(window.innerWidth <= 768);
      if (window.innerWidth > 768) setShowPanel(false);
    };
    window.addEventListener("resize", handleResize);
    return () => window.removeEventListener("resize", handleResize);
  }, []);

  
    // Fetch available dates when page loads
  useEffect(() => {

    const fetchAvailability = async () => {

      try {

        // Get JWT token from local storage
        const token = localStorage.getItem("token");

        const response = await fetch(
          "http://localhost:3000/api/Bookings/available-dates",
          {
            headers: {
              Authorization: `Bearer ${token}`
            }
          }
        );

        if (!response.ok) {
          throw new Error("Failed to load available dates");
        }

        const data = await response.json();

        // Save data into state
        setAvailability(data.availability);
        setStartDate(data.startDate);
        setEndDate(data.endDate);

      } catch (error) {
        console.log(error);
      }
    };

    fetchAvailability();

  }, []);





  // Runs whenever user picks a date
  const handleDateChange = (e) => {

    const date = e.target.value;

    setSelectedDate(date);

    // Convert date string into day number
    const day = new Date(date).getDate();

    // Find times for that day
    const times = availability[day] || [];

    setAvailableTimes(times);

    // Clear previous time selection
    setSelectedTime("");
  };

    // Submit booking
  const handleSubmit = async (e) => {

    e.preventDefault();

    try {

      const token = localStorage.getItem("token");

      const response = await fetch(
        "http://localhost:3000/api/AddBooking/AddBooking",
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${token}`
          },
          body: JSON.stringify({
            idNumber : formData.idNumber,
            appointmentDate: selectedDate,
            appointmentTime: selectedTime,
            appointmentType : formData.appointmentType
          })
        }
      );

      const data = await response.json();

      if (!response.ok) {
        alert(data.message);
        return;
      }

      alert("Booking created successfully!");

    } catch (error) {

      console.log(error);

    }
  };


  // Form state
  const [formData, setFormData] = useState({
    idNumber: '',
    appointmentType: '',
    date: '',
    time: '',
    notes: ''
  });

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData({ ...formData, [name]: value });
  };



  return (
    <div className="booking-page">
      <Navbar
        togglePanel={() => setShowPanel((prev) => !prev)}
        isMobile={isMobile}
      />

      {showPanel && isMobile && <Panel closePanel={() => setShowPanel(false)} />}

      <div className="booking-container">
        <div className="booking-form glass">
          <h2 className="booking-title"> Book Appointment</h2>
          <p className="booking-sub">Fill in the details below to schedule your visit.</p>

          <form onSubmit={handleSubmit}>
            {/* ID Number */}
            <div className="form-group">
              <label>ID Number</label>
              <input
                type="text"
                name="idNumber"
                value={formData.idNumber}
                onChange={handleChange}
                placeholder="8001015009087"
                maxLength="13"
                required
              />
            </div>

            {/* Appointment Type */}
            <div className="form-group">
              <label>Appointment Type</label>
              <select
                name="appointmentType"
                value={formData.appointmentType}
                onChange={handleChange}
                required
              >
                <option value="">Select type</option>
                <option value="General Checkup">General Checkup</option>
                <option value="Follow-up">Follow-up</option>
                <option value="Vaccination">Vaccination</option>
                <option value="Cold / Flu Symptoms">Cold / Flu Symptoms</option>
                <option value="Test Results">Test Results</option>
              </select>
            </div>

            {/* Date */}
  <div className="form-group">


  <label>Date</label>
  <input
    type="date"
    name="date"
    value={selectedDate}
    onChange={handleDateChange}
   min={startDate}   
   max={endDate}  
   required
  />
</div>



   
   {/* Time slots */}
      <div>

        <h4>Available Times</h4>
        
        {availableTimes.length === 0 ? (
          <p>No slots available</p>
        ) : (

          availableTimes.map((time) => (
         <div className='btn-all-times'>
            <button
              type="button"
              key={time}
              className='btn-time'
              onClick={() => setSelectedTime(time)}
              style={{
                margin: "5px",
                backgroundColor:
                selectedTime === time ? "green" : "#ddd"
              }}
            >
              {time}
            </button>
            </div>

          ))

        )}

      </div>

            {/* Notes */}
            <div className="form-group">

              <label > Notes </label>
              <textarea
                name="notes"
                value={formData.notes}
                onChange={handleChange}
                placeholder="Any additional information..."
                rows="3"
              />
            </div>

            {/* Buttons */}
            <div className="form-actions">
            <button type="submit" className="btn-submit" onClick={handleSubmit}>Book Appointment</button>
              <a  href='/' className="btn-cancel"> Cancel </a>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}

export default AddBooking;