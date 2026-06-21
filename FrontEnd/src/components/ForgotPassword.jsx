import { useState , useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { validateForgotPassword, sanitizeInput } from '../services/validationService';
import { isLoggedIn } from '../services/AuthServices';


function ForgotPassword() {
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [errors, setErrors] = useState({});
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState('');
  const [apiError, setApiError] = useState('');

    useEffect(() => {
      if (isLoggedIn()) {
          navigate('/dashboard');
      }
  }, [navigate]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSuccess('');
    setApiError('');

    const sanitizedEmail = sanitizeInput(email);
    const { isValid, errors: validationErrors } = validateForgotPassword({ email: sanitizedEmail });

    if (!isValid) {
      setErrors(validationErrors);
      return;
    }

    setLoading(true);

    try {
      const response = await fetch('http://localhost:3000/api/Auth/Forgot', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email: sanitizedEmail })
      });

      const data = await response.json();

      if (response.ok) {
        setSuccess(data.message);
        setTimeout(() => {
          navigate('/verifyForgot?email=' + encodeURIComponent(sanitizedEmail));
        }, 2000);
      } else {
        setApiError(data.message || 'Failed to send reset code.');
      }
    } catch (error) {
      setApiError('Network error. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-box">
      <h2>Forgot Password</h2>
      <p className="auth-subtext">Enter your email and we'll send you a reset code.</p>

      <form onSubmit={handleSubmit}>
        <div className="field">
          <label>Email</label>
          <input
            type="email"
            value={email}
            onChange={(e) => {
              setEmail(e.target.value);
              if (errors.email) setErrors({});
            }}
            placeholder="hello@example.com"
            className={errors.email ? 'error' : ''}
          />
          {errors.email && <span className="field-error">{errors.email}</span>}
        </div>

        {apiError && <div className="error-message">{apiError}</div>}
        {success && <div className="success-message">{success}</div>}

        <button type="submit" disabled={loading}>
          {loading ? 'Sending...' : 'Send Reset Code'}
        </button>
      </form>
    </div>
  );
}

export default ForgotPassword;