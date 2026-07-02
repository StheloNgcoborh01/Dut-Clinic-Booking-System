import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import Navbar from '../../components/Navbar';
import Panel from '../../context/Panel';
import '../../styles/TodaysBookings.css';

function TodaysBookings() {
  const navigate = useNavigate();
  const [showPanel, setShowPanel] = useState(false);
  const [isMobile, setIsMobile] = useState(window.innerWidth <= 768);
  const [bookings, setBookings] = useState([]);
  const [loading, setLoading] = useState(true);
  const [date, setDate] = useState('');
  const [actionLoading, setActionLoading] = useState(null); // Track which booking is being updated

  useEffect(() => {
    const handleResize = () => {
      setIsMobile(window.innerWidth <= 768);
      if (window.innerWidth > 768) setShowPanel(false);
    };
    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, []);

  const fetchTodaysBookings = async () => {
    try {
      const token = localStorage.getItem('token');
      const response = await fetch('http://localhost:3000/api/Admin/TodaysBookings', {
        headers: { 'Authorization': `Bearer ${token}` }
      });
      const data = await response.json();
      if (response.ok) {
        setBookings(data.bookings || []);
        setDate(data.date || '');
      }
    } catch (error) {
      console.error('Failed to fetch bookings:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchTodaysBookings();
  }, []);

  const handleAction = async (id, action) => {

    const messages = {
    complete: 'Mark this appointment as Completed?',
    noshow: 'Mark this appointment as No-Attender?',
    cancel: 'Are you sure you want to CANCEL this appointment?'
  };

  // Show confirmation popup
  if (!window.confirm(messages[action])) {
    return; // User clicked Cancel
  }

    setActionLoading(id);
    try {
      const token = localStorage.getItem('token');
      let endpoint = '';

      if (action === 'complete') {
        endpoint = `http://localhost:3000/api/Admin/markComplete/${id}`;
      } else if (action === 'noshow') {
        endpoint = `http://localhost:3000/api/Admin/MarkNoAttended/${id}`;
      } else if (action === 'cancel') {
        endpoint = `http://localhost:3000/api/Admin/admincancel/${id}`;
      }

      const response = await fetch(endpoint, {
        method: action === 'cancel' ? 'GET' : 'PATCH',
        headers: { 'Authorization': `Bearer ${token}` }
      });

      const data = await response.json();

      if (response.ok) {
        // Refresh the list after successful action
        fetchTodaysBookings();
      } else {
        alert(data.message || 'Action failed');
      }
    } catch (error) {
      console.error('Action failed:', error);
      alert('Network error. Please try again.');
    } finally {
      setActionLoading(null);
    }
  };

  const formatDate = (dateStr) => {
    const date = new Date(dateStr);
    return date.toLocaleDateString('en-ZA', {
      day: '2-digit',
      month: 'short',
      year: 'numeric'
    });
  };

  const getStatusColor = (status) => {
    switch (status) {
      case 'Upcoming': return 'upcoming';
      case 'Completed': return 'completed';
      case 'Cancelled': return 'cancelled';
      case 'Not Attended': return 'noshow';
      default: return '';
    }
  };

  return (
    <div className="todays-bookings-page">
      <Navbar togglePanel={() => setShowPanel((prev) => !prev)} isMobile={isMobile} />
      {showPanel && isMobile && <Panel closePanel={() => setShowPanel(false)} />}

      <div className="todays-bookings-container">
        <div className="page-header">
          <h1 className="page-title">📅 Today's Appointments</h1>
          <span className="page-date">{date}</span>
        </div>

        {loading ? (
          <p className="loading-text">Loading today's bookings...</p>
        ) : bookings.length === 0 ? (
          <div className="empty-state">
            <p>No appointments for today.</p>
          </div>
        ) : (
          <div className="table-wrapper">
            <table className="bookings-table">
              <thead>
                <tr>
                  <th>Time</th>
                  <th>Patient</th>
                  <th>Type</th>
                  <th>Reference</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {bookings.map((booking) => (
                  <tr key={booking.id}>
                    <td>{booking.appointmentTime}</td>
                    <td>{booking.name} {booking.surname}</td>
                    <td>{booking.appointmentType}</td>
                    <td><span className="ref">{booking.reference}</span></td>
                    <td>
                      <span className={`status-badge ${getStatusColor(booking.status)}`}>
                        {booking.status}
                      </span>
                    </td>
                    <td>
                      <div className="action-buttons">
                        {booking.status === 'Upcoming' && (
                          <>
                            <button
                              className="action-btn complete"
                              onClick={() => handleAction(booking.id, 'complete')}
                              disabled={actionLoading === booking.id}
                              title="Mark as Completed"
                            >
                              ✅ Complete
                            </button>
                            <button
                              className="action-btn noshow"
                              onClick={() => handleAction(booking.id, 'noshow')}
                              disabled={actionLoading === booking.id}
                              title="Mark as No-Show"
                            >
                              ❌ Not-attended
                            </button>
                            <button
                              className="action-btn cancel"
                              onClick={() => handleAction(booking.id, 'cancel')}
                              disabled={actionLoading === booking.id}
                              title="Cancel Booking"
                            >
                              🗑️ Cancel booking
                            </button>
                          </>
                        )}
                        {booking.status === 'Completed' && <span className="action-done">✅ Done</span>}
                        {booking.status === 'Not Attended' && <span className="action-done noshow-text">❌ No-Show</span>}
                        {booking.status === 'Cancelled' && <span className="action-done cancelled-text">🗑️ Cancelled</span>}
                      </div>
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

export default TodaysBookings;