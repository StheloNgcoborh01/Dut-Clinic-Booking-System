import { useState, useEffect } from 'react';
import Navbar from '../../components/Navbar';
import Panel from '../../context/Panel';
import '../../styles/Messages.css';

function Messages() {
  const [showPanel, setShowPanel] = useState(false);
  const [isMobile, setIsMobile] = useState(window.innerWidth <= 768);
  const [messages, setMessages] = useState([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState('all');
  const [selectedMessage, setSelectedMessage] = useState(null);

  useEffect(() => {
    const handleResize = () => {
      setIsMobile(window.innerWidth <= 768);
      if (window.innerWidth > 768) setShowPanel(false);
    };
    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, []);

  const fetchMessages = async () => {
    try {
      const token = localStorage.getItem('token');
      let endpoint = '';

      if (filter === 'all') {
        endpoint = 'http://localhost:3000/api/AdminContact/allMessages';
      } else if (filter === 'unread') {
        endpoint = 'http://localhost:3000/api/AdminContact/UnreadMessages';
      } else if (filter === 'read') {
        endpoint = 'http://localhost:3000/api/AdminContact/ReadMessages';
      }

      const response = await fetch(endpoint, {
        headers: { 'Authorization': `Bearer ${token}` }
      });

      const data = await response.json();

      if (response.ok) {
        setMessages(data.allMessages || []);
      } else {
        console.error('API error:', data);
      }
    } catch (error) {
      console.error('Fetch error:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchMessages();
  }, [filter]);

  const toggleRead = async (id, currentStatus) => {
    const newStatus = !currentStatus;
    const action = newStatus ? 'Mark as Read' : 'Mark as Unread';
    
    if (!window.confirm(`Mark this message as ${newStatus ? 'Read' : 'Unread'}?`)) {
      return;
    }

    try {
      const token = localStorage.getItem('token');
      const response = await fetch(`http://localhost:3000/api/AdminContact/MarkMessage/toggleRead/${id}`, {
        method: 'PATCH',
        headers: { 'Authorization': `Bearer ${token}` }
      });

      if (response.ok) {
        fetchMessages();
      } else {
        alert('Failed to update message status.');
      }
    } catch (error) {
      console.error('Failed to toggle read:', error);
      alert('Network error. Please try again.');
    }
  };

  const openMessage = (msg) => {
    setSelectedMessage(msg);
  };

  const closeMessage = () => {
    setSelectedMessage(null);
  };

  const formatDate = (dateStr) => {
    const date = new Date(dateStr);
    return date.toLocaleDateString('en-ZA', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const getStatusClass = (isRead) => {
    return isRead ? 'read' : 'unread';
  };

  const getStatusText = (isRead) => {
    return isRead ? 'Read' : 'Unread';
  };

  return (
    <div className="messages-page">
      <Navbar togglePanel={() => setShowPanel((prev) => !prev)} isMobile={isMobile} />
      {showPanel && isMobile && <Panel closePanel={() => setShowPanel(false)} />}

      <div className="messages-container">
        <div className="page-header">
          <h1 className="page-title">📧 Messages</h1>
        </div>

        <div className="filter-tabs">
          <button
            className={filter === 'all' ? 'active' : ''}
            onClick={() => setFilter('all')}
          >
            All
          </button>
          <button
            className={filter === 'unread' ? 'active' : ''}
            onClick={() => setFilter('unread')}
          >
            Unread
          </button>
          <button
            className={filter === 'read' ? 'active' : ''}
            onClick={() => setFilter('read')}
          >
            Read
          </button>
        </div>

        {loading ? (
          <p className="loading-text">Loading messages...</p>
        ) : messages.length === 0 ? (
          <div className="empty-state">
            <p>No messages found.</p>
          </div>
        ) : (
          <div className="table-wrapper">
            <table className="messages-table">
              <thead>
                <tr>
                  <th>From</th>
                  <th>Message</th>
                  <th>Date</th>
                  <th>Status</th>
                  <th>Action</th>
                </tr>
              </thead>
              <tbody>
                {messages.map((msg) => (
                  <tr key={msg.id} className={msg.isRead ? '' : 'unread-row'}>
                    <td>
                      <strong>{msg.name}</strong>
                      {!msg.isAnonymous && <span className="email-tag">{msg.email}</span>}
                      {msg.isAnonymous && <span className="anonymous-tag">Anonymous</span>}
                    </td>
                    <td
                      className="message-text"
                      onClick={() => openMessage(msg)}
                      style={{ cursor: 'pointer' }}
                    >
                      {msg.message.length > 80 ? msg.message.substring(0, 80) + '...' : msg.message}
                    </td>
                    <td>{formatDate(msg.createdAt)}</td>
                    <td>
                      <span className={`status-badge ${getStatusClass(msg.isRead)}`}>
                        {getStatusText(msg.isRead)}
                      </span>
                    </td>
                    <td>
                      <button className="view-btn" onClick={() => openMessage(msg)}>Read Message</button>
                      <button
                        className="toggle-read-btn"
                        onClick={() => toggleRead(msg.id, msg.isRead)}
                      >
                        {msg.isRead ? 'Mark Unread' : 'Mark Read'}
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Message Modal */}
      {selectedMessage && (
        <div className="modal-overlay" onClick={closeMessage}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <button className="modal-close" onClick={closeMessage}>✕</button>

            <div className="modal-header">
              <h2 className="modal-name">{selectedMessage.name}</h2>
              {selectedMessage.isAnonymous && <span className="modal-anonymous">Anonymous</span>}
              {!selectedMessage.isAnonymous && (
                <span className="modal-email">{selectedMessage.email}</span>
              )}
              <span className="modal-date">{formatDate(selectedMessage.createdAt)}</span>
            </div>

            <div className="modal-body">
              <p>{selectedMessage.message}</p>
            </div>

            <div className="modal-footer">
              <button
                className="modal-toggle-btn"
                onClick={() => {
                  toggleRead(selectedMessage.id, selectedMessage.isRead);
                  closeMessage();
                }}
              >
                {selectedMessage.isRead ? 'Mark as Unread' : 'Mark as Read'}
              </button>
              <button className="modal-close-btn" onClick={closeMessage}>Close</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default Messages;