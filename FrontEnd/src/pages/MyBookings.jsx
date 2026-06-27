import { useState, useEffect } from 'react';
import Navbar from '../components/Navbar';
import Panel from '../context/Panel';
import '../styles/MyBookings.css';

function MyBookings() {
  const [showPanel, setShowPanel] = useState(false);
  const [isMobile, setIsMobile] = useState(window.innerWidth <= 768);
  const [bookings, setBookings] = useState([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState('all');

  useEffect(() => {
    const handleResize = () => {
      setIsMobile(window.innerWidth <= 768);
      if (window.innerWidth > 768) setShowPanel(false);
    };
    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, []);

  const fetchBookings = async () => {
    try {
      console.log('Token:', localStorage.getItem('token'));
      const token = localStorage.getItem('token');
      let endpoint = '';

      if (filter === 'all') {
        endpoint = 'http://localhost:3000/api/AddBooking/MyBookings';
      } else if (filter === 'upcoming') {
        endpoint = 'http://localhost:3000/api/AddBooking/FutureBookings';
      } else if (filter === 'past') {
        endpoint = 'http://localhost:3000/api/AddBooking/MyPastBookings';
      }

      const response = await fetch(endpoint, {
        headers: { Authorization: `Bearer ${token}` },
      });
      const data = await response.json();
      if (response.ok) {
        setBookings(data.bookings || []);
      }
    } catch (error) {
      console.error('Failed to fetch bookings:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
      
    fetchBookings();
  }, [filter]);

  const formatDate = (dateStr) => {
    const date = new Date(dateStr);
    return date.toLocaleDateString('en-ZA', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
    });
  };

  const getStatusClass = (status) => {
    switch (status) {
      case 'Upcoming':
        return 'upcoming';
      case 'Completed':
        return 'completed';
      case 'Cancelled':
        return 'cancelled';
      case 'No-Show':
        return 'noshow';
      default:
        return '';
    }
  };

  return (
    <div className="mybookings-page">
      <Navbar togglePanel={() => setShowPanel((prev) => !prev)} isMobile={isMobile} />

      {showPanel && isMobile && <Panel closePanel={() => setShowPanel(false)} />}

      <div className="mybookings-container">
        <h1 className="mybookings-title">My Bookings</h1>

        {/* Tabs */}
        <div className="filter-tabs">
          <button className={filter === 'all' ? 'active' : ''} onClick={() => setFilter('all')}>
            All
          </button>
          <button className={filter === 'upcoming' ? 'active' : ''} onClick={() => setFilter('upcoming')}>
            Upcoming
          </button>
          <button className={filter === 'past' ? 'active' : ''} onClick={() => setFilter('past')}>
            Past
          </button>
        </div>

        {loading ? (
          <p className="mybookings-loading">Loading your bookings...</p>
        ) : bookings.length === 0 ? (
          <div className="mybookings-empty">
            <p>You don't have any bookings yet.</p>
            <a href="/AddBooking" className="empty-btn">Book Appointment</a>
          </div>
        ) : (
          <div className="table-wrapper">
            <table className="booking-table">
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Time</th>
                  <th>Type</th>
                  <th>Reference</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {bookings.map((booking) => (
                  <tr key={booking.id} className={booking.status === 'Completed' || booking.status === 'No-Show' ? 'past-row' : ''}>
                    <td>{formatDate(booking.appointmentDate)}</td>
                    <td>{booking.appointmentTime}</td>
                    <td>{booking.appointmentType}</td>
                    <td>
                      <span className="booking-ref">{booking.reference}</span>
                    </td>
                    <td>
                      <span className={`status-badge ${getStatusClass(booking.status)}`}>
                        {booking.status}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}

export default MyBookings;