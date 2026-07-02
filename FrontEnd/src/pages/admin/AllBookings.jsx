import { useState, useEffect } from 'react';
import Navbar from '../../components/Navbar';
import Panel from '../../context/Panel';
import '../../styles/AllBookings.css';

function AllBookings() {
  const [showPanel, setShowPanel] = useState(false);
  const [isMobile, setIsMobile] = useState(window.innerWidth <= 768);
  const [bookings, setBookings] = useState([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');

  useEffect(() => {
    const handleResize = () => {
      setIsMobile(window.innerWidth <= 768);
      if (window.innerWidth > 768) setShowPanel(false);
    };
    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, []);

  const fetchAllBookings = async () => {
    try {
      const token = localStorage.getItem('token');
      const response = await fetch('http://localhost:3000/api/Admin/allBookings', {
        headers: { 'Authorization': `Bearer ${token}` }
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
    fetchAllBookings();
  }, []);

  // Filter bookings by search term (reference, name, type)
  const filteredBookings = bookings.filter((booking) => {
    const search = searchTerm.toLowerCase();
    return (
      booking.reference?.toLowerCase().includes(search) ||
      booking.name?.toLowerCase().includes(search) ||
      booking.surname?.toLowerCase().includes(search) ||
      booking.appointmentType?.toLowerCase().includes(search)
    );
  });

  const formatDate = (dateStr) => {
    const date = new Date(dateStr);
    return date.toLocaleDateString('en-ZA', {
      day: '2-digit',
      month: 'short',
      year: 'numeric'
    });
  };

  const getStatusClass = (status) => {
    switch (status) {
      case 'Upcoming': return 'upcoming';
      case 'Completed': return 'completed';
      case 'Cancelled': return 'cancelled';
      case 'Not Attended': return 'noshow';
      default: return '';
    }
  };

  return (
    <div className="all-bookings-page">
      <Navbar togglePanel={() => setShowPanel((prev) => !prev)} isMobile={isMobile} />
      {showPanel && isMobile && <Panel closePanel={() => setShowPanel(false)} />}

      <div className="all-bookings-container">
        <div className="page-header">
          <h1 className="page-title"> All Bookings</h1>
          <span className="booking-count">{bookings.length} total</span>
        </div>

        {/* Search Bar */}
        <div className="search-container">
          <input
            type="text"
            className="search-input"
            placeholder=" Search by reference, patient name, or type..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </div>

        {loading ? (
          <p className="loading-text">Loading all bookings...</p>
        ) : filteredBookings.length === 0 ? (
          <div className="empty-state">
            <p>{searchTerm ? 'No bookings match your search.' : 'No bookings found.'}</p>
          </div>
        ) : (
          <div className="table-wrapper">
            <table className="bookings-table">
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Time</th>
                  <th>Patient</th>
                  <th>Type</th>
                  <th>Reference</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {filteredBookings.map((booking) => (
                  <tr key={booking.id}>
                    <td>{formatDate(booking.appointmentDate)}</td>
                    <td>{booking.appointmentTime}</td>
                    <td>{booking.name} {booking.surname}</td>
                    <td>{booking.appointmentType}</td>
                    <td><span className="ref">{booking.reference}</span></td>
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

export default AllBookings;