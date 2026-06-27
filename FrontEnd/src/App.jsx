import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import './styles/Auth.css';
import LoginForm from './components/LoginForm';
import RegisterForm from './components/RegisterForm';
import VerifyForm from './components/VerifyForm';
import ForgotPassword from './components/ForgotPassword';
import VerifyForgot from './components/VerifyForgot';
import PasswordReset from './components/PasswordReset';
import ProtectedRoute from './services/ProtectedRoute';
import NavBar from './components/NavBar';
import Home from './pages/Home';
import './styles/Navbar.css';
import AddBooking from './pages/AddBooking.jsx';
import MyBookings from './pages/MyBookings';


function App() {
  return (
    <BrowserRouter>
      <div className="App">
        <Routes>

          <Route path="/login" element={ <LoginForm /> } />
          <Route path="/register" element={<RegisterForm />} />
          <Route path="/verify" element={<VerifyForm />} />
          <Route path="/forgot" element={<ForgotPassword />} />
          <Route path="/verifyForgot" element={<VerifyForgot />} />
          <Route path ="passwordReset" element = { <PasswordReset /> } /> 
          <Route path ="home" element = {  <Home /> } />
          <Route path = "/" element = { <Home /> } />

          <Route path ="AddBooking" element =  {<ProtectedRoute> <AddBooking /> </ProtectedRoute> }    /> 
          <Route path="/bookings" element=    { <ProtectedRoute>  <MyBookings />   </ProtectedRoute> } />
        </Routes>
      </div>
    </BrowserRouter>
  );
}

export default App;