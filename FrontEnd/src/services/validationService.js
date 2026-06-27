// ============================================
// 1. INDIVIDUAL VALIDATION FUNCTIONS
// ============================================

// Check if field is empty
export const isEmpty = (value) => {
  return value === null || value === undefined || value.trim() === '';
};

// Email format validation
export const isValidEmail = (email) => {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(email);
};

// Password strength: min 8 chars, uppercase, lowercase, number
export const isStrongPassword = (password) => {
  return password.length >= 8 &&
         /[a-z]/.test(password) &&
         /[A-Z]/.test(password) &&
         /[0-9]/.test(password);
};


export const doPasswordsMatch = (password, confirmPassword) => {
  return password === confirmPassword;
};


export const sanitizeInput = (value) => {
  return value.trim();
};



// Login validation
export const validateLogin = (data) => {
  const errors = {};

  if (isEmpty(data.email)) {
    errors.email = 'Email is required';
  } else if (!isValidEmail(data.email)) {
    errors.email = 'Invalid email address';
  }


  if (isEmpty(data.password)) {
    errors.password = 'Password is required';
  }

  return { isValid: Object.keys(errors).length === 0, errors };
};


export const validateRegister = (data) => {
  const errors = {};

  if (isEmpty(data.fname)) errors.fname = 'First name is required';
  if (isEmpty(data.lname)) errors.lname = 'Last name is required';

  if (isEmpty(data.email)) {
    errors.email = 'Email is required';
  } else if (!isValidEmail(data.email)) {
    errors.email = 'Invalid email address';
  }

  if (isEmpty(data.password)) {
    errors.password = 'Password is required';
  } else if (!isStrongPassword(data.password)) {
    errors.password = 'Password must be at least 8 characters with uppercase, lowercase, and a number';
  }

  if (data.confirmPassword !== undefined) {
    if (!doPasswordsMatch(data.password, data.confirmPassword)) {
      errors.confirmPassword = 'Passwords do not match';
    }
  }

  return { isValid: Object.keys(errors).length === 0, errors };
};

export const validateForgotPassword = (data) => {
  const errors = {};

  if (isEmpty(data.email)) {
    errors.email = 'Email is required';
  } else if (!isValidEmail(data.email)) {
    errors.email = 'Invalid email address';
  }

  return { isValid: Object.keys(errors).length === 0, errors };
};

export const validateVerifyCode = (data) => {
  const errors = {};

  if (isEmpty(data.code)) {
    errors.code = 'Verification code is required';
  } else if (!/^\d{6}$/.test(data.code)) {
    errors.code = 'Enter a valid 6-digit code';
  }

  return { isValid: Object.keys(errors).length === 0, errors };
};


export const validateResetPassword = (data) => {
  const errors = {};

  if (isEmpty(data.password)) {
    errors.password = 'Password is required';
  } else if (!isStrongPassword(data.password)) {
    errors.password = 'Password must be at least 8 characters with uppercase, lowercase, and a number';
  }

  if (!doPasswordsMatch(data.password, data.confirmPassword)) {
    errors.confirmPassword = 'Passwords do not match';
  }

  return { isValid: Object.keys(errors).length === 0, errors };
};