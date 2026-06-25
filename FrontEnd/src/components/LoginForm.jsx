import { useState , useEffect  } from 'react';
import { useNavigate } from 'react-router-dom';
import { validateLogin, sanitizeInput } from '../services/validationService';
import { isLoggedIn } from '../services/AuthServices';
import { Password } from '@mui/icons-material';

function LoginForm() {
  const navigate = useNavigate();
  const [formData, setFormData] = useState({ email: '', password: '' });
  const [errors, setErrors] = useState({});
  const [loading, setLoading] = useState(false);



  useEffect(() => {
    if (isLoggedIn()) {
        navigate('/');
    }
}, [navigate]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData({ ...formData, [name]: value });
    // Clear error for this field when user types
    if (errors[name]) setErrors({ ...errors, [name]: '' });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    // Sanitize inputs
    const sanitizedData = {
      email: sanitizeInput(formData.email),
      password: sanitizeInput(formData.password)
    };

    // Validate
    const { isValid, errors: validationErrors } = validateLogin(sanitizedData);

    if (!isValid) {
      setErrors(validationErrors);
      return; // Stop if validation fails
    }

    setLoading(true);

    try {
      // Send to backend
      const response = await fetch('http://localhost:3000/api/Auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
         Email : sanitizedData.email,
         Password : sanitizedData.password
        })
      });

      const data = await response.json();

      if (response.ok) {
        localStorage.setItem('token', data.token);
        navigate('/dashboard');
      } else {
        setErrors({ general: data.message || 'Login failed' });
      }
    } catch (error) {
      setErrors({ general: 'Network error. Please try again.' });
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-box">
      <h2>Sign In</h2>
      <form onSubmit={handleSubmit}>
        <div className="field">
          <label>Email</label>
          <input
            type="email"
            name="email"
            value={formData.email}
            onChange={handleChange}
            placeholder="hello@example.com"
          />
          {errors.email && <span className="field-error">{errors.email}</span>}
        </div>

        <div className="field">
          <label>Password</label>
          <input
            type="password"
            name="password"
            value={formData.password}
            onChange={handleChange}
            placeholder="········"
          />
          {errors.password && <span className="field-error">{errors.password}</span>}
        </div>

        {errors.general && <div className="error-message">{errors.general}</div>}

        <button type="submit" disabled={loading}>
          {loading ? 'Logging in...' : 'Login'}
        </button>
          <div className="signup-link">
         Don't Have an Account? <a href="register">Sign Up</a>
        </div>
      </form>
    </div>
  );
}

export default LoginForm;