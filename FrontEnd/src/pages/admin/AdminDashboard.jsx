import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import Navbar from '../../components/Navbar';
import Panel from '../../context/Panel';
import '../../styles/AdminDashboard.css';

function AdminDashboard() {
  const navigate = useNavigate();
  const [showPanel, setShowPanel] = useState(false);
  const [isMobile, setIsMobile] = useState(window.innerWidth <= 768);
  const [stats, setStats] = useState({
    todayCount: 0,
    totalBookings: 0,
    unreadMessages: 0,
    totalUsers: 0
  });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const handleResize = () => {
      setIsMobile(window.innerWidth <= 768);
      if (window.innerWidth > 768) setShowPanel(false);
    };
    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, []);

  useEffect(() => {
    const fetchStats = async () => {
      try {
        const token = localStorage.getItem('token');
        
        // Fetch all stats in parallel
        const [todayRes, totalRes, unreadRes, usersRes] = await Promise.all([
          fetch('https://dut-clinic-booking-system-y9d7.onrender.com/api/Admin/TodaysBookings', {
            headers: { 'Authorization': `Bearer ${token}` }
          }),
          fetch('https://dut-clinic-booking-system-y9d7.onrender.com/api/Admin/totalBookingsCount', {
            headers: { 'Authorization': `Bearer ${token}` }
          }),
          fetch('https://dut-clinic-booking-system-y9d7.onrender.com/api/Admin/unreadMessagesCount', {
            headers: { 'Authorization': `Bearer ${token}` }
          }),
          fetch('https://dut-clinic-booking-system-y9d7.onrender.com/api/Admin/allUsers', {
            headers: { 'Authorization': `Bearer ${token}` }
          })
        ]);

        const todayData = await todayRes.json();
        const totalData = await totalRes.json();
        const unreadData = await unreadRes.json();
        const usersData = await usersRes.json();

        setStats({
          todayCount: todayData.count || 0,
          totalBookings: totalData.totalBookings || 0,
          unreadMessages: unreadData.unreadCount || 0,
          totalUsers: usersData.count || 0
        });
      } catch (error) {
        console.error('Failed to fetch stats:', error);
      } finally {
        setLoading(false);
      }
    };
    fetchStats();
  }, []);

  const cards = [
    {
      id: 'today',
      title: "Today's Bookings",
      count: stats.todayCount,
      icon: '📅',
      color: '#354e78',
      path: '/admin/todays-bookings'
    },
    {
      id: 'total',
      title: 'Total Bookings',
      count: stats.totalBookings,
      icon: '📋',
      color: '#2c7da0',
      path: '/admin/all-bookings'
    },
    {
      id: 'messages',
      title: 'Unread Messages',
      count: stats.unreadMessages,
      icon: '📧',
      color: '#e67e22',
      path: '/admin/messages'
    },
    {
      id: 'users',
      title: 'Total Users',
      count: stats.totalUsers,
      icon: '👥',
      color: '#27ae60',
      path: '/admin/users'
    }
  ];

  return (
    <div className="admin-dashboard-page">
      <Navbar togglePanel={() => setShowPanel((prev) => !prev)} isMobile={isMobile} />
      {showPanel && isMobile && <Panel closePanel={() => setShowPanel(false)} />}

      <div className="admin-dashboard-container">
        <h1 className="dashboard-title"> Admin Dashboard</h1>

        {loading ? (
          <p className="dashboard-loading">Loading dashboard...</p>
        ) : (
          <div className="dashboard-cards">
            {cards.map((card) => (
              <div
                key={card.id}
                className="dashboard-card"
                style={{ borderBottom: `4px solid ${card.color}` }}
                onClick={() => navigate(card.path)}
              >
                <div className="card-icon">{card.icon}</div>
                <div className="card-content">
                  <h3 className="card-title">{card.title}</h3>
                  <p className="card-count">{card.count}</p>
                </div>
              </div>
            ))}
          </div>
        )}

        {/* Quick Links */}
        <div className="quick-actions">
          <button onClick={() => navigate('/admin/todays-bookings')} className="quick-btn">
           View Today
          </button>
          <button onClick={() => navigate('/admin/all-bookings')} className="quick-btn">
             All Bookings
          </button>
          <button onClick={() => navigate('/admin/messages')} className="quick-btn">
             Messages
          </button>
        </div>
      </div>
    </div>
  );
}

export default AdminDashboard;