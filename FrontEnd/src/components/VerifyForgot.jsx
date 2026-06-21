import { useState, useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { validateVerifyCode, sanitizeInput } from '../services/validationService';
import { isLoggedIn } from '../services/AuthServices';

function VerifyForgot() {
  const navigate = useNavigate();
  const location = useLocation();
  const [email, setEmail] = useState('');
  const [code, setCode] = useState('');
  const [errors, setErrors] = useState({});
  const [loading, setLoading] = useState(false);
  const [apiError, setApiError] = useState('');
  const [success, setSuccess] = useState('');
  const [message, setMessage] = useState('');

    useEffect(() => {
      if (isLoggedIn()) {
          navigate('/dashboard');
      }
  }, [navigate]);

  useEffect(() => {
    const params = new URLSearchParams(location.search);
    const emailParam = params.get('email');
    if (emailParam) setEmail(emailParam);
  }, [location]);



  const handleSubmit = async (e) => {
    e.preventDefault();
    setApiError('');
    setSuccess('');

    const sanitizedCode = sanitizeInput(code);
    const { isValid, errors: validationErrors } = validateVerifyCode({ code: sanitizedCode });

    if (!isValid) {
      setErrors(validationErrors);
      return;
    }

    setLoading(true);

    try {
      const response = await fetch('http://localhost:3000/api/Auth/verifyForgot', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ 
          Email : email
          
          , VerifyCode: sanitizedCode })
      });

      const data = await response.json();

      if (response.ok) {
        setSuccess(' Code verified!');
       localStorage.setItem('token', data.resetToken);
        setTimeout(() => {
          navigate('/login?email=' + encodeURIComponent(email));
        }, 1500);
      } else {
        setApiError(data.message || 'Invalid or expired code.');
      }
    } catch (error) {
      setApiError('Network error. Please try again.');
    } finally {
      setLoading(false);
    }
  };

    const HandleReset = async (e) => {
    e.preventDefault();
    setLoading(true);
    setMessage("");

    try {
      const response = await fetch(
        "http://localhost:3000/api/Auth/ResendEmail",
        {
          method: "PATCH",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({
            Email: email,
          }),
        },
      );

      const data = await response.json();

      if (response.ok) {
         navigate('/verify?email=' + email);


      } else {
        setMessage(data.message || "Please try again.");
      }
    } catch (error) {
      setMessage("Network error. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-box">
      <h2>Verify Reset Code</h2>
      <p className="auth-subtext">Enter the 6-digit code sent to <strong>{email}</strong></p>

      <form onSubmit={handleSubmit}>
        <div className="field">
          <label>Verification Code</label>
          <input
            type="text"
            value={code}
            onChange={(e) => {
              setCode(e.target.value);
              if (errors.code) setErrors({});
            }}
            placeholder="123456"
            maxLength="6"
            className={errors.code ? 'error' : ''}
          />
          {errors.code && <span className="field-error">{errors.code}</span>}
        </div>

        {apiError && <div className="error-message">{apiError}</div>}
        {success && <div className="success-message">{success}</div>}
      
        <button type="submit" disabled={loading}>
          {loading ? 'Verifying...' : 'Verify Code'}
        </button>


            <div className="signup-link">
          Didn't receive code? <a onClick={HandleReset}>Resend</a>
        </div>
      </form>
    </div>
  );
}

export default VerifyForgot;