import { useState , useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { validateRegister, sanitizeInput  } from '../services/validationService';
import { isLoggedIn } from '../services/AuthServices';


function RegisterForm({ onRegisterSuccess }) {
  
  
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    fname: '',
    lname: '',
    email: '',
    password: '',
    confirmPassword: ''
  });
  const [errors, setErrors] = useState({});
  const [loading, setLoading] = useState(false);
  const [apiError, setApiError] = useState('');

    useEffect(() => {
      if (isLoggedIn()) {
          navigate('/dashboard');
      }
  }, [navigate]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData({ ...formData, [name]: value });
    if (errors[name]) setErrors({ ...errors, [name]: '' });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setApiError('');

    // Sanitize inputs
    const sanitizedData = {
      fname: sanitizeInput(formData.fname),
      lname: sanitizeInput(formData.lname),
      email: sanitizeInput(formData.email),
      password: sanitizeInput(formData.password),
      confirmPassword: sanitizeInput(formData.confirmPassword)
    };

    // Validate
    const { isValid, errors: validationErrors } = validateRegister(sanitizedData);

    if (!isValid) {
      setErrors(validationErrors);
      return;
    }

    setLoading(true);

    try {
  const response = await fetch('http://localhost:3000/api/Auth/register', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      fname: sanitizedData.fname,
      lname: sanitizedData.lname,
      email: sanitizedData.email,
      password: sanitizedData.password
    })
  });

  const data = await response.json();

  if (response.ok) {
    // Success
    navigate('/verify?email=' + encodeURIComponent(sanitizedData.email));
  } else {
    // Show the backend's error message
    setApiError(data.message || 'Something went wrong. Please try again.');
  }
} catch (error) {
  // Network error (backend not running)
  setApiError('Network error. Please check your connection.');
} finally {
  setLoading(false);
}

  };

  return (
    <div className="auth-box">
      <h2>Register</h2>
      <form onSubmit={handleSubmit}>
        <div className="name-row">
          <div className="field">
            <label>First Name</label>
            <input
              name="fname"
              value={formData.fname}
              onChange={handleChange}
              placeholder="Asanda"
              className={errors.fname ? 'error' : ''}
            />
            {errors.fname && <span className="field-error">{errors.fname}</span>}
          </div>
          <div className="field">
            <label>Last Name</label>
            <input
              name="lname"
              value={formData.lname}
              onChange={handleChange}
              placeholder="Ngcobo"
              className={errors.lname ? 'error' : ''}
            />
            {errors.lname && <span className="field-error">{errors.lname}</span>}
          </div>
        </div>

        <div className="field">
          <label>Email</label>
          <input
            name="email"
            type="email"
            value={formData.email}
            onChange={handleChange}
            placeholder="hello@example.com"
            className={errors.email ? 'error' : ''}
          />
          {errors.email && <span className="field-error">{errors.email}</span>}
        </div>

        <div className="field">
          <label>Password</label>
          <input
            name="password"
            type="password"
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
            name="confirmPassword"
            type="password"
            value={formData.confirmPassword}
            onChange={handleChange}
            placeholder="········"
            className={errors.confirmPassword ? 'error' : ''}
          />
          {errors.confirmPassword && <span className="field-error">{errors.confirmPassword}</span>}
        </div>

        {apiError && <div className="error-message">{apiError}</div>}

        <button type="submit" disabled={loading}>
          {loading ? 'Registering...' : 'Register'}
        </button>

             <div className="signup-link">
         Already Have an Account? <a href="login">Log In</a>
        </div>
      </form>
    </div>
  );
}

export default RegisterForm;