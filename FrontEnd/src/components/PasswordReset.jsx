import { useState, useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { validateResetPassword, sanitizeInput } from '../services/validationService';

function ResetPassword() {
  const navigate = useNavigate();
  const location = useLocation();
  const [email, setEmail] = useState('');
  const [formData, setFormData] = useState({
    password: '',
    confirmPassword: ''
  });
  const [errors, setErrors] = useState({});
  const [loading, setLoading] = useState(false);
  const [apiError, setApiError] = useState('');
  const [success, setSuccess] = useState('');

  const [resetToken , setResetToken] = ('');

  // Get email from URL
  useEffect(() => {
    const params = new URLSearchParams(location.search);
    const emailParam = params.get('email');
    if (emailParam) setEmail(emailParam);
  }, [location]);

  useEffect(() => {
    var resetToken = localStorage.getItem("resetToken");

    if (resetToken)  setResetToken(resetToken);
     else {
        navigate('/forgot');
     }

  });

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData({ ...formData, [name]: value });
    if (errors[name]) setErrors({ ...errors, [name]: '' });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setApiError('');
    setSuccess('');

    const sanitizedData = {
      password: sanitizeInput(formData.password),
      confirmPassword: sanitizeInput(formData.confirmPassword)
    };

    const { isValid, errors: validationErrors } = validateResetPassword(sanitizedData);

    if (!isValid) {
      setErrors(validationErrors);
      return;
    }

    setLoading(true);


    try {
      const response = await fetch('https://dut-clinic-booking-system-y9d7.onrender.com/api/Auth/reset-password', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          ResetToken : resetToken,
          Password: sanitizedData.password
        })
      });

      const data = await response.json();

      if (response.ok) {
        setSuccess('Password reset successfully!');
        setTimeout(() => {
          navigate('/login');
        }, 2000);
      } else {
        setApiError(data.message || 'Failed to reset password.');
      }
    } catch (error) {
      setApiError('Network error. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-box">
      <h2>Reset Password</h2>
      <p className="auth-subtext">
        Enter your new password for <strong>{email}</strong>
      </p>

      <form onSubmit={handleSubmit}>
        <div className="field">
          <label>New Password</label>
          <input
            type="password"
            name="password"
            value={formData.password}
            onChange={handleChange}
            placeholder="········"
            className={errors.password ? 'error' : ''}
          />
          {errors.password && <span className="field-error">{errors.password}</span>}
        </div>

        <div className="field">
          <label>Confirm Password</label>
          <input
            type="password"
            name="confirmPassword"
            value={formData.confirmPassword}
            onChange={handleChange}
            placeholder="········"
            className={errors.confirmPassword ? 'error' : ''}
          />
          {errors.confirmPassword && <span className="field-error">{errors.confirmPassword}</span>}
        </div>

        {apiError && <div className="error-message">{apiError}</div>}
        {success && <div className="success-message">{success}</div>}

        <button type="submit" disabled={loading}>
          {loading ? 'Resetting...' : 'Reset Password'}
        </button>

        

        <div className="auth-link">
          <a href="/login">← Back to Login</a>
        </div>
      </form>
    </div>
  );
}

export default ResetPassword;