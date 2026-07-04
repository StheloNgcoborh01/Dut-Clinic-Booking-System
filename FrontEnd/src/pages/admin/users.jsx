import { useState, useEffect } from 'react';
import Navbar from '../../components/Navbar';
import Panel from '../../context/Panel';
import '../../styles/Users.css';

function Users() {
  const [showPanel, setShowPanel] = useState(false);
  const [isMobile, setIsMobile] = useState(window.innerWidth <= 768);
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState('all'); // 'all' | 'admins'
  const [togglingId, setTogglingId] = useState(null);
  const [showPasswordModal, setShowPasswordModal] = useState(false);
  const [selectedUserId, setSelectedUserId] = useState(null);
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');

  useEffect(() => {
    const handleResize = () => {
      setIsMobile(window.innerWidth <= 768);
      if (window.innerWidth > 768) setShowPanel(false);
    };
    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, []);

  const fetchUsers = async () => {
    try {
      const token = localStorage.getItem('token');
      const response = await fetch('https://dut-clinic-booking-system-y9d7.onrender.com/api/Admin/allUsers', {
        headers: { 'Authorization': `Bearer ${token}` }
      });
      const data = await response.json();
      if (response.ok) {
        setUsers(data.users || []);
      }
    } catch (error) {
      console.error('Failed to fetch users:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchUsers();
  }, []);

  const filteredUsers = users.filter(user => {
    if (filter === 'admins') return user.isAdmin;
    return true;
  });

  const handleToggleAdmin = (userId) => {
    setSelectedUserId(userId);
    setPassword('');
    setError('');
    setShowPasswordModal(true);
  };

  const confirmToggleAdmin = async () => {
    if (!password.trim()) {
      setError('Please enter your password.');
      return;
    }

    setTogglingId(selectedUserId);
    try {
      const token = localStorage.getItem('token');
      const response = await fetch(`https://dut-clinic-booking-system-y9d7.onrender.com/api/Admin/toggleadmin/${selectedUserId}`, {
        method: 'PATCH',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({ password })
      });

      const data = await response.json();

      if (response.ok) {
        setShowPasswordModal(false);
        fetchUsers();
        alert(data.message);
      } else {
        setError(data.message || 'Failed to toggle admin status.');
      }
    } catch (error) {
      console.error('Failed to toggle admin:', error);
      setError('Network error. Please try again.');
    } finally {
      setTogglingId(null);
    }
  };

  return (
    <div className="users-page">
      <Navbar togglePanel={() => setShowPanel((prev) => !prev)} isMobile={isMobile} />
      {showPanel && isMobile && <Panel closePanel={() => setShowPanel(false)} />}

      <div className="users-container">
        <div className="page-header">
          <h1 className="page-title">👥 Users</h1>
          <span className="user-count">{filteredUsers.length} users</span>
        </div>

        {/* Tabs */}
        <div className="filter-tabs">
          <button
            className={filter === 'all' ? 'active' : ''}
            onClick={() => setFilter('all')}
          >
            All Users
          </button>
          <button
            className={filter === 'admins' ? 'active' : ''}
            onClick={() => setFilter('admins')}
          >
            Admins
          </button>
        </div>

        {loading ? (
          <p className="loading-text">Loading users...</p>
        ) : filteredUsers.length === 0 ? (
          <div className="empty-state">
            <p>No users found.</p>
          </div>
        ) : (
          <div className="table-wrapper">
            <table className="users-table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Email</th>
                  <th>Verified</th>
                  <th>Role</th>
                  <th>Action</th>
                </tr>
              </thead>
              <tbody>
                {filteredUsers.map((user) => (
                  <tr key={user.id}>
                    <td><strong>{user.fname} {user.lname}</strong></td>
                    <td>{user.email}</td>
                    <td>
                      <span className={`verified-badge ${user.isVerified ? 'verified' : 'unverified'}`}>
                        {user.isVerified ? '✅ Verified' : '❌ Unverified'}
                      </span>
                    </td>
                    <td>
                      <span className={`role-badge ${user.isAdmin ? 'admin' : 'user'}`}>
                        {user.isAdmin ? 'Admin' : 'User'}
                      </span>
                    </td>
                    <td>
                      <button
                        className={`toggle-admin-btn ${user.isAdmin ? 'remove' : 'add'}`}
                        onClick={() => handleToggleAdmin(user.id)}
                        disabled={togglingId === user.id}
                      >
                        {togglingId === user.id ? '...' : user.isAdmin ? 'Remove Admin' : 'Make Admin'}
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Password Modal */}
      {showPasswordModal && (
        <div className="modal-overlay" onClick={() => setShowPasswordModal(false)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <button className="modal-close" onClick={() => setShowPasswordModal(false)}>✕</button>
            <h3 className="modal-title">🔐 Confirm Action</h3>
            <p className="modal-subtext">Enter your admin password to confirm this action.</p>

            <div className="modal-input-group">
              <label>Password</label>
              <input
                type="password"
                placeholder="Enter your password..."
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                onKeyDown={(e) => e.key === 'Enter' && confirmToggleAdmin()}
              />
            </div>

            {error && <p className="modal-error">{error}</p>}

            <div className="modal-footer">
              <button className="modal-cancel-btn" onClick={() => setShowPasswordModal(false)}>
                Cancel
              </button>
              <button className="modal-confirm-btn" onClick={confirmToggleAdmin}>
                Confirm
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default Users;