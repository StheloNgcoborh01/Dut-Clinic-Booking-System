import { useState , useEffect} from 'react';
import { useNavigate } from 'react-router-dom';
import Navbar from '../components/Navbar';
import Panel from '../context/Panel';
import '../styles/Contact.css';

function Contact() {
  const navigate = useNavigate();
  const [showPanel, setShowPanel] = useState(false);
  const [isMobile, setIsMobile] = useState(window.innerWidth <= 768);
  const [formData, setFormData] = useState({
    message: '',
    isAnonymous: false
  });
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState('');
  const [error, setError] = useState('');


    useEffect(() => {
      const handleResize = () => {
        setIsMobile(window.innerWidth <= 768);
        if (window.innerWidth > 768) setShowPanel(false);
      };
      window.addEventListener("resize", handleResize);
      return () => window.removeEventListener("resize", handleResize);
    }, []);

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFormData({
      ...formData,
      [name]: type === 'checkbox' ? checked : value
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setSuccess('');
    setError('');

    try {
      const token = localStorage.getItem('token');
      const response = await fetch('http://localhost:3000/api/Contacts/SendMessage', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({
          message: formData.message,
          isAnonymous: formData.isAnonymous
        })
      });

      const data = await response.json();

      if (response.ok) {
        setSuccess('Message sent successfully!');
        setFormData({ message: '', isAnonymous: false });
        setTimeout(() => navigate('/'), 2000);
      } else {
        setError(data.message || 'Failed to send message.');
      }
    } catch (err) {
      setError('Network error. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="contact-page">
      <Navbar
        togglePanel={() => setShowPanel((prev) => !prev)}
        isMobile={isMobile}
      />
      {showPanel && isMobile && <Panel closePanel={() => setShowPanel(false)} />}

      <div className="contact-container">
        <div className="contact-form glass">
          <h1 className="contact-title">  Contact Us</h1>
          <p className="contact-sub">Send us a message and we'll get back to you.</p>

          <form onSubmit={handleSubmit}>
            <div className="form-group">
              <label>Message</label>
              <textarea
                name="message"
                value={formData.message}
                onChange={handleChange}
                placeholder="Write your message here..."
                rows="5"
                required
              />
            </div>

            <div className="form-group checkbox-group">
              <label>
                <input
                  type="checkbox"
                  name="isAnonymous"
                  checked={formData.isAnonymous}
                  onChange={handleChange}
                  className="checkbox-label"
                />
                Send anonymously
              </label>
              <span className="checkbox-hint">Your name and email will be hidden.</span>
            </div>

            {success && <div className="success-message">{success}</div>}
            {error && <div className="error-message">{error}</div>}

            <button type="submit" className="submit-btn" disabled={loading}>
              {loading ? 'Sending...' : 'Send Message'}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}

export default Contact;